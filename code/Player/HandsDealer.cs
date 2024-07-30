using System;
using Sandbox;
using Sandbox.Physics;
using Sandbox.VR;



public sealed class HandsDealer : Component
{
	[Property] public GameObject Head {get;set;}

	[Property] public SkinnedModelRenderer LeftArmRenderer {get;set;}
	[Property] public SkinnedModelRenderer RightArmRenderer {get;set;}

	[Property] public GameObject HandRawL {get;set;}
	[Property] public GameObject HandRawR {get;set;}
	[Property] public GameObject HandSmoothL {get;set;}
	[Property] public GameObject HandSmoothR {get;set;}
	OneEuroFilter<Vector3> positionFilterL;
	OneEuroFilter<Vector3> positionFilterR;

	OneEuroFilter<Rotation> rotationFilterL;
	OneEuroFilter<Rotation> rotationFilterR;
	public float filterFrequency = 10.0f;
	public float filterMinCutoff = 1.0f;
	public float filterBeta = 0.0f;
	public float filterDcutoff = 1.0f;

	[Property] public GameObject HandTargetL {get;set;}
	[Property] public GameObject HandTargetR {get;set;}

	[Property] public Rigidbody FinalPhysL {get;set;}
	GrabPointFinder grabPointsL;
	[Property] public Rigidbody PhysPointL {get;set;}
	[Property] public Rigidbody FinalPhysR {get;set;}
	GrabPointFinder grabPointsR;
	[Property] public Rigidbody PhysPointR {get;set;}
	
	[Property] public GameObject LArmParent {get;set;}
	[Property] public GameObject LArmAnchor {get;set;}

	[Property] public GameObject RArmAnchor {get;set;}
	[Property] public GameObject RArmParent {get;set;}
	[Property] public float AnchorDistance {get;set;}

	float leftGripAmount;
	float rightGripAmount;

	Vector3 handTargetLocalStartPosR;
	Vector3 handTargetLocalStartPosL;

	Rotation handTargetLocalStartRotR;
	Rotation handTargetLocalStartRotL;


	protected override async void OnStart()
	{
		handTargetLocalStartPosL = HandTargetL.Transform.LocalPosition;
		handTargetLocalStartRotL = HandTargetL.Transform.LocalRotation;
		handTargetLocalStartPosR = HandTargetR.Transform.LocalPosition;
		handTargetLocalStartRotR = HandTargetR.Transform.LocalRotation;
		positionFilterL = new OneEuroFilter<Vector3>(filterFrequency);
		positionFilterR = new OneEuroFilter<Vector3>(filterFrequency);
		rotationFilterL = new OneEuroFilter<Rotation>(filterFrequency);
		rotationFilterR = new OneEuroFilter<Rotation>(filterFrequency);

		grabPointsL = FinalPhysL.Components.Get<GrabPointFinder>();
		grabPointsR = FinalPhysR.Components.Get<GrabPointFinder>();

		//await Task.Frame();
		FinalPhysL.Transform.Position = PhysPointL.Transform.Position;
		FinalPhysL.Transform.Rotation = PhysPointL.Transform.Rotation;

		var p1 = new PhysicsPoint(PhysPointL.PhysicsBody,PhysPointL.Transform.Position);
		var p2 = new PhysicsPoint(FinalPhysL.PhysicsBody,PhysPointR.Transform.Position);

		Sandbox.Physics.FixedJoint fixedJoint = PhysicsJoint.CreateFixed(p1,p2);
		fixedJoint.SpringLinear = new PhysicsSpring(100, 0);
		fixedJoint.SpringAngular = new PhysicsSpring(100, 0);
  
		

		FinalPhysR.Transform.Position = PhysPointR.Transform.Position;
		FinalPhysR.Transform.Rotation = PhysPointR.Transform.Rotation;

		p1 = new PhysicsPoint(PhysPointR.PhysicsBody,PhysPointL.Transform.Position);
		p2 = new PhysicsPoint(FinalPhysR.PhysicsBody,PhysPointR.Transform.Position);

		fixedJoint = PhysicsJoint.CreateFixed(p1,p2);
		fixedJoint.SpringLinear = new PhysicsSpring(100, 0);
		fixedJoint.SpringAngular = new PhysicsSpring(100, 0);
	}
	bool HoldingObject;
	protected override void OnPreRender()
	{
		HandSmoothing();

		SetIK();

		if(Input.VR != null) Inputs();

		StretchPrevent();

		Physics();
	}

	void SetIK()
	{
		RightArmRenderer.SetIk("hand_right", HandTargetR.Transform.World);
		LeftArmRenderer.SetIk("hand_left", HandTargetL.Transform.World);
	}
	void Physics()
	{
		FinalPhysL.Transform.Rotation = PhysPointL.Transform.Rotation;
		FinalPhysR.Transform.Rotation = PhysPointR.Transform.Rotation;
	}

	bool lHolding;
	bool rHolding;

	Sandbox.Physics.FixedJoint grabJointLeft;
	Sandbox.Physics.FixedJoint grabJointRight;

	GameObject grabPointL;
	GameObject grabPointR;
	
	void Inputs()
	{
		leftGripAmount = MathX.Lerp(leftGripAmount,(Input.VR.LeftHand.Grip.Value+Input.VR.LeftHand.Trigger.Value)/2,Time.Delta*10);

		rightGripAmount = MathX.Lerp(rightGripAmount,(Input.VR.RightHand.Grip.Value+Input.VR.RightHand.Trigger.Value)/2,Time.Delta*10);
		LeftArmRenderer.Set("HoldAmount", leftGripAmount);
		RightArmRenderer.Set("HoldAmount", rightGripAmount);

		(grabJointLeft, lHolding, grabPointL) = Grabber(grabPointsL,FinalPhysL, Input.VR.LeftHand, grabJointLeft, lHolding, grabPointL, HandTargetL, handTargetLocalStartPosL, handTargetLocalStartRotL);
		(grabJointRight, rHolding, grabPointR) = Grabber(grabPointsR,FinalPhysR, Input.VR.RightHand, grabJointRight, rHolding, grabPointR, HandTargetR, handTargetLocalStartPosR, handTargetLocalStartRotR);
	}
	void StretchPrevent()
	{
		RArmParent.Transform.Rotation = Rotation.LookAt(HandTargetR.Transform.Position-RArmParent.Transform.Position);
		LArmParent.Transform.Rotation = Rotation.LookAt(HandTargetL.Transform.Position-LArmParent.Transform.Position);

		RightArmRenderer.GameObject.Parent.Transform.LocalPosition = Vector3.Zero.WithX(
			MathX.Clamp(Vector3.DistanceBetween(RArmAnchor.Transform.Position,HandTargetR.Transform.Position) - AnchorDistance,0,100)
		);
		LeftArmRenderer.GameObject.Parent.Transform.LocalPosition = Vector3.Zero.WithX(
			MathX.Clamp(Vector3.DistanceBetween(LArmAnchor.Transform.Position,HandTargetL.Transform.Position) - AnchorDistance,0,100)
		);
	}
	void HandSmoothing()
	{
		//positionFilterL.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothL.Transform.Position = HandRawL.Transform.Position;

		//rotationFilterL.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothL.Transform.Rotation = HandRawL.Transform.Rotation;

		// Right
		//positionFilterR.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothR.Transform.Position = HandRawR.Transform.Position;

		//rotationFilterR.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothR.Transform.Rotation = HandRawR.Transform.Rotation;
	}


	(Sandbox.Physics.FixedJoint fixedJoint, bool holding, GameObject grabPoint ) 
	Grabber(GrabPointFinder pointFinder, Rigidbody handPhys, VRController hand, Sandbox.Physics.FixedJoint fixedJointRef, bool holdingRef, GameObject grabPointRef, GameObject handTarget, Vector3 handLocalPos, Rotation handLocalRot)
	{
		if(holdingRef)
		{
			if(hand.Grip < 0.5f)
			{
				fixedJointRef.Remove();
				handTarget.SetParent(handPhys.GameObject);
				handTarget.Transform.LocalPosition = handLocalPos;
				handTarget.Transform.LocalRotation = handLocalRot;
				grabPointRef.Tags.Remove("grabbed");
				return (null,false,null);
			}
			return (fixedJointRef, holdingRef, grabPointRef);
		}

		if(pointFinder.GrabbablePoints.Count == 0) return (fixedJointRef, holdingRef, grabPointRef);
		
		GameObject closest = null;
		float closestDis = 500;
		foreach(GameObject p in pointFinder.GrabbablePoints)
		{
			if(p.Tags.Contains("grabbed")) continue;
			float distance = Vector3.DistanceBetween(p.Transform.Position,pointFinder.Transform.Position);
			if(distance > closestDis) continue;
			closest = p;
			closestDis = distance;
		}

		if(closest == null) return (fixedJointRef, holdingRef, grabPointRef);
		
		
		Gizmo.Draw.SolidSphere(closest.Transform.Position,0.5f);

		if(hand.Grip > 0.75f)
		{
			//handPhys.Transform.Position = closest.Transform.Position;
			//handPhys.Transform.Rotation = closest.Transform.Rotation;
			var p1 = new PhysicsPoint( handPhys.PhysicsBody, handPhys.Transform.Position );
			var p2 = new PhysicsPoint( closest.Parent.Parent.Components.Get<Rigidbody>().PhysicsBody, closest.Parent.Transform.Position);
			Sandbox.Physics.FixedJoint newFixedJoint = PhysicsJoint.CreateFixed(p1,p2);
			handTarget.SetParent(closest);
			handTarget.Transform.LocalPosition = Vector3.Zero;
			handTarget.Transform.LocalRotation = Rotation.Identity;
			closest.Tags.Add("grabbed");
			return (newFixedJoint, true, closest);
		}


		return (fixedJointRef, holdingRef, grabPointRef);
	}
}
