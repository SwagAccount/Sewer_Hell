using System;
using Sandbox;
using Sandbox.Physics;
using Sandbox.VR;



public sealed class HandsDealer : Component
{
	[Property] public GameObject Head {get;set;}

	[Property] public SkinnedModelRenderer LeftArmRenderer {get;set;}
	[Property] public SkinnedModelRenderer RightArmRenderer {get;set;}

	[Property] public GameObject HandSkeletonL {get;set;}
	[Property] public GameObject HandSkeletonR {get;set;}

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


	protected override void OnStart()
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
	protected override void OnUpdate()
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
		//FinalPhysL.Transform.Rotation = PhysPointL.Transform.Rotation;
		//FinalPhysR.Transform.Rotation = PhysPointR.Transform.Rotation;
	}

	bool lHolding;
	bool rHolding;

	Sandbox.Physics.FixedJoint grabJointLeft;
	Sandbox.Physics.FixedJoint grabJointRight;

	GameObject grabPointL;
	GameObject grabPointR;

	HandPos currentHandPosL;
	HandPos currentHandPosR;
	
	void Inputs()
	{
		leftGripAmount = MathX.Lerp(leftGripAmount,(Input.VR.LeftHand.Grip.Value+Input.VR.LeftHand.Trigger.Value)/2,Time.Delta*10);

		rightGripAmount = MathX.Lerp(rightGripAmount,(Input.VR.RightHand.Grip.Value+Input.VR.RightHand.Trigger.Value)/2,Time.Delta*10);
		LeftArmRenderer.Set("HoldAmount", leftGripAmount);
		RightArmRenderer.Set("HoldAmount", rightGripAmount);

		(grabJointLeft, lHolding, grabPointL, currentHandPosL) = Grabber(grabPointsL,FinalPhysL, Input.VR.LeftHand, HandSkeletonL, grabJointLeft, lHolding, grabPointL, HandTargetL, handTargetLocalStartPosL, handTargetLocalStartRotL, currentHandPosL);
		(grabJointRight, rHolding, grabPointR, currentHandPosR) = Grabber(grabPointsR,FinalPhysR, Input.VR.RightHand, HandSkeletonR, grabJointRight, rHolding, grabPointR, HandTargetR, handTargetLocalStartPosR, handTargetLocalStartRotR, currentHandPosR);
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


	(Sandbox.Physics.FixedJoint fixedJoint, bool holding, GameObject grabPoint, HandPos currentHandPos) 
	Grabber(GrabPointFinder pointFinder, Rigidbody handPhys, VRController hand, GameObject handSkeleton, Sandbox.Physics.FixedJoint fixedJointRef, bool holdingRef, GameObject grabPointRef, GameObject handTarget, Vector3 handLocalPos, Rotation handLocalRot, HandPos currentHandPosRef)
	{
		if(holdingRef)
		{
			if(hand.Grip < 0.5f)
			{
				fixedJointRef.Body2.Velocity = fixedJointRef.Body1.Velocity;
				fixedJointRef.Remove();
				handTarget.SetParent(handPhys.GameObject);
				handTarget.Transform.LocalPosition = handLocalPos;
				handTarget.Transform.LocalRotation = handLocalRot;
				MakeAnimated(handSkeleton);
				grabPointRef.Parent.Tags.Remove("grabbed");
				return (null,false,null, null);
			}
			CopyTransformRecursive(currentHandPosRef.wristObject,handSkeleton, Vector3.One,new Angles(1,1,1));

			return (fixedJointRef, holdingRef, grabPointRef,currentHandPosRef);
		}

		if(pointFinder.GrabbablePoints.Count == 0) return (fixedJointRef, holdingRef, grabPointRef,currentHandPosRef);
		
		GameObject closest = null;
		float closestDis = 500;
		foreach(GameObject p in pointFinder.GrabbablePoints)
		{
			if(p.Tags.Contains("grabbed")) continue;
			float distance = Vector3.DistanceBetween(p.Transform.Position,pointFinder.searchPoint);
			if(distance > closestDis) continue;
			closest = p;
			closestDis = distance;
		}

		if(closest == null) return (fixedJointRef, holdingRef, grabPointRef,currentHandPosRef);
		
		
		Gizmo.Draw.SolidSphere(closest.Parent.Transform.Position,0.5f);

		if(hand.Grip > 0.75f)
		{
			//handPhys.Transform.Position = closest.Transform.Position;
			//handPhys.Transform.Rotation = closest.Transform.Rotation;
			AlignByChild(closest.Parent.Parent, closest, handTarget);

			var p1 = new PhysicsPoint( handPhys.PhysicsBody, handPhys.Transform.Position );
			var p2 = new PhysicsPoint( closest.Parent.Parent.Components.Get<Rigidbody>().PhysicsBody, closest.Parent.Transform.Position);
			Sandbox.Physics.FixedJoint newFixedJoint = PhysicsJoint.CreateFixed(p1,p2);

			//AlignBy(closest.Parent.Parent, closest, handPhys.GameObject, handTarget);
			
			handTarget.SetParent(closest);
			handTarget.Transform.LocalPosition = Vector3.Zero;
			handTarget.Transform.LocalRotation = Rotation.Identity;
			HandPos handPos = closest.Components.Get<HandPos>();
			closest.Parent.Tags.Add("grabbed");
			MakeProcedual(handSkeleton);
			return (newFixedJoint, true, closest, handPos);
		}


		return (fixedJointRef, holdingRef, grabPointRef,currentHandPosRef);
	}

	public static void MakeProcedual(GameObject target)
    {
        for (int i = 0; i < target.Children.Count; i++)
        {
			target.Children[i].Flags = GameObjectFlags.ProceduralBone;
            MakeProcedual(target.Children[i]);
        }
    }

	public static void MakeAnimated(GameObject target)
    {
        for (int i = 0; i < target.Children.Count; i++)
        {
			target.Children[i].Flags = GameObjectFlags.Bone;
            MakeAnimated(target.Children[i]);
        }
    }

	public static void CopyTransformRecursive(GameObject target, GameObject set, Vector3 posMod, Angles angMod)
    {
        if (target.Children.Count != set.Children.Count )
        {
            Log.Error("Children not the same");
            return;
        }

        for (int i = 0; i < target.Children.Count; i++)
        {
            GameObject targetChild = target.Children[i];
            GameObject setChild = set.Children[i];

            setChild.Transform.LocalPosition = targetChild.Transform.LocalPosition * posMod;
			Vector3 modifiedAngles = targetChild.Transform.LocalRotation.Angles().AsVector3()*angMod.AsVector3();
            setChild.Transform.LocalRotation = new Angles(modifiedAngles.x,modifiedAngles.y,modifiedAngles.z); //new Angles(targetChild.Transform.LocalRotation.Angles().pitch*-1,targetChild.Transform.LocalRotation.Angles().yaw*-1,targetChild.Transform.LocalRotation.Angles().roll*1);
            CopyTransformRecursive(targetChild, setChild, posMod, angMod);
        }
    }

	//from halley1 on discussions.unity.com
	public static void AlignByChild(GameObject assembly, GameObject feature, GameObject station)
    {
            assembly.Transform.Rotation = station.Transform.Rotation * (assembly.Transform.Rotation.Inverse * feature.Transform.Rotation).Inverse;

            assembly.Transform.Position =
                station.Transform.Position +
                (assembly.Transform.Position - feature.Transform.Position);
    }
}
