using System;
using Sandbox;
using Sandbox.Physics;
using Sandbox.VR;

namespace trollface;

public sealed class HandsDealer : Component
{
	[Property] public GameObject Head {get;set;}
	[Property] public GameObject VRSpace {get;set;}

	[Property] public SkinnedModelRenderer LeftArmRenderer {get;set;}
	[Property] public SkinnedModelRenderer RightArmRenderer {get;set;}

	[Property] public GameObject HandSkeletonL {get;set;}
	[Property] public GameObject HandSkeletonR {get;set;}

	[Property] public GameObject HandRawL {get;set;}
	[Property] public GameObject HandRawR {get;set;}
	[Property] public GameObject HandSmoothL {get;set;}
	[Property] public GameObject HandSmoothR {get;set;}

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

	[Property] public float leftBreak {get;set;}
	[Property] public float rightBreak {get;set;}

	float leftGripAmount;
	float rightGripAmount;

	Vector3 handTargetLocalStartPosR;
	Vector3 handTargetLocalStartPosL;

	Rotation handTargetLocalStartRotR;
	Rotation handTargetLocalStartRotL;

	Sandbox.Physics.FixedJoint leftHandFJoint;
	Sandbox.Physics.FixedJoint rightHandFJoint;
	Sandbox.Physics.SpringJoint leftHandSJoint;
	Sandbox.Physics.SpringJoint rightHandSJoint;

	ChunkDealer chunkDealer;

	protected override void OnStart()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();

		handTargetLocalStartPosL = HandTargetL.Transform.LocalPosition;
		handTargetLocalStartRotL = HandTargetL.Transform.LocalRotation;
		handTargetLocalStartPosR = HandTargetR.Transform.LocalPosition;
		handTargetLocalStartRotR = HandTargetR.Transform.LocalRotation;

		grabPointsL = FinalPhysL.Components.Get<GrabPointFinder>();
		grabPointsR = FinalPhysR.Components.Get<GrabPointFinder>();

		//await Task.Frame();
		FinalPhysL.Transform.Position = PhysPointL.Transform.Position;
		FinalPhysL.Transform.Rotation = PhysPointL.Transform.Rotation;

		var p1 = new PhysicsPoint(PhysPointL.PhysicsBody,PhysPointL.Transform.Position);
		var p2 = new PhysicsPoint(FinalPhysL.PhysicsBody,PhysPointR.Transform.Position);

		leftHandFJoint = PhysicsJoint.CreateFixed(p1,p2);
		leftHandFJoint.SpringLinear = new PhysicsSpring(100, 5);
		leftHandFJoint.SpringAngular = new PhysicsSpring(100, 5);

		FinalPhysR.Transform.Position = PhysPointR.Transform.Position;
		FinalPhysR.Transform.Rotation = PhysPointR.Transform.Rotation;

		p1 = new PhysicsPoint(PhysPointR.PhysicsBody,PhysPointL.Transform.Position);
		p2 = new PhysicsPoint(FinalPhysR.PhysicsBody,PhysPointR.Transform.Position);

		rightHandFJoint = PhysicsJoint.CreateFixed(p1,p2);
		rightHandFJoint.SpringLinear = new PhysicsSpring(100, 5);
		rightHandFJoint.SpringAngular = new PhysicsSpring(100, 5);
		Rigidbody lArmParentRB = LArmParent.Components.Get<Rigidbody>();
		Rigidbody rArmParentRB = RArmParent.Components.Get<Rigidbody>();


		p1 = new PhysicsPoint(lArmParentRB.PhysicsBody, Vector3.Zero);
		p2 = new PhysicsPoint(FinalPhysL.PhysicsBody, Vector3.Zero);
		leftHandSJoint = PhysicsJoint.CreateSpring(p1,p2, AnchorDistance/2, AnchorDistance);
		
		p1 = new PhysicsPoint(rArmParentRB.PhysicsBody, Vector3.Zero);
		p2 = new PhysicsPoint(FinalPhysR.PhysicsBody, Vector3.Zero);
		rightHandSJoint = PhysicsJoint.CreateSpring(p1, p2, AnchorDistance/2, AnchorDistance);

		leftHandSJoint.IsActive = false;
		rightHandSJoint.IsActive = false;
		
	}
	protected override void OnPreRender()
	{
		HandSmoothing();

		if(Input.VR != null) Inputs();

		StretchPrevent();

		Physics();

		BrokenArms();

		SetIK();
	}

	void BrokenArms()
	{
		leftBreak-=Time.Delta;
		rightBreak-=Time.Delta;
		
		leftHandFJoint.IsActive = leftBreak <= 0;
		rightHandFJoint.IsActive = rightBreak <= 0;
		leftHandSJoint.IsActive = leftBreak > 0;
		rightHandSJoint.IsActive = rightBreak > 0;

		FinalPhysL.LinearDamping =  leftBreak > 0 ? 1 : 0;
		FinalPhysR.LinearDamping = rightBreak > 0 ? 1 : 0;

		FinalPhysL.AngularDamping = leftBreak > 0  ? 1 : 0;
		FinalPhysR.AngularDamping = rightBreak > 0  ? 1 : 0;
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
	bool reLHold;
	bool rHolding;
	bool reRHold;

	Sandbox.Physics.FixedJoint grabJointLeft;
	Sandbox.Physics.FixedJoint grabJointRight;

	GameObject grabPointL;
	GameObject grabPointR;

	HandPos currentHandPosL;
	HandPos currentHandPosR;

	float lastLeftTrigger;
	float lastRightTrigger;
	public VRHandGrabber leftHandGrabber;
	public VRHandGrabber rightHandGrabber;
	bool lPointing;
	bool rPointing;
	void Inputs()
	{
		
		leftGripAmount = MathX.Lerp(leftGripAmount,(Input.VR.LeftHand.Grip.Value+Input.VR.LeftHand.Trigger.Value)/2,Time.Delta*10);

		rightGripAmount = MathX.Lerp(rightGripAmount,(Input.VR.RightHand.Grip.Value+Input.VR.RightHand.Trigger.Value)/2,Time.Delta*10);

		lPointing = Input.VR.LeftHand.Trigger.Value < 0.1f && Input.VR.LeftHand.GetFingerValue(FingerValue.ThumbCurl) > 0.5f && Input.VR.LeftHand.Grip.Value > 0.75f;
		rPointing = Input.VR.RightHand.Trigger.Value < 0.1f && Input.VR.RightHand.GetFingerValue(FingerValue.ThumbCurl) > 0.5f && Input.VR.RightHand.Grip.Value > 0.75f;

		LeftArmRenderer.Set("HoldAmount", leftGripAmount);
		LeftArmRenderer.Set("Point", lPointing);

		RightArmRenderer.Set("HoldAmount", rightGripAmount);
		RightArmRenderer.Set("Point", rPointing);

		

		leftHandGrabber = new VRHandGrabber
		{
			VRSpace = VRSpace
		};
		rightHandGrabber = new VRHandGrabber
		{
			VRSpace = VRSpace
		};

		(grabJointLeft, lHolding, grabPointL, currentHandPosL, reLHold) = leftHandGrabber.Grabber(grabPointsL, FinalPhysL, Input.VR.LeftHand, HandSkeletonL, grabJointLeft, lHolding, grabPointL, HandTargetL, handTargetLocalStartPosL, handTargetLocalStartRotL, currentHandPosL, reLHold, lastLeftTrigger);
		(grabJointRight, rHolding, grabPointR, currentHandPosR, reRHold) = rightHandGrabber.Grabber(grabPointsR, FinalPhysR, Input.VR.RightHand, HandSkeletonR, grabJointRight, rHolding, grabPointR, HandTargetR, handTargetLocalStartPosR, handTargetLocalStartRotR, currentHandPosR, reRHold, lastRightTrigger);


		grabPointsL.Enabled = !lHolding;
		grabPointsR.Enabled = !rHolding;

		lastLeftTrigger = Input.VR.LeftHand.Trigger.Value;
		lastRightTrigger = Input.VR.RightHand.Trigger.Value;
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

		HandSmoothL.Transform.Position = HandRawL.Transform.Position;
		HandSmoothL.Transform.Rotation = HandRawL.Transform.Rotation;
		HandSmoothR.Transform.Position = HandRawR.Transform.Position;
		HandSmoothR.Transform.Rotation = HandRawR.Transform.Rotation;
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
            setChild.Transform.LocalRotation = new Angles(modifiedAngles.x,modifiedAngles.y,modifiedAngles.z);
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

	public static void CopyTransformRecursiveLerp(GameObject target1, GameObject target2, GameObject set, Vector3 posMod, Angles angMod, float frac)
    {
        if (target1.Children.Count != set.Children.Count && target2.Children.Count != set.Children.Count)
        {
            Log.Error("Children not the same");
            return;
        }

        for (int i = 0; i < set.Children.Count; i++)
        {
            GameObject target1Child = target1.Children[i];
            GameObject target2Child = target2.Children[i];
            GameObject setChild = set.Children[i];

            setChild.Transform.LocalPosition = Vector3.Lerp(target1Child.Transform.LocalPosition, target2Child.Transform.LocalPosition, frac) * posMod;
			Vector3 modifiedAngles = Vector3.Lerp(target1Child.Transform.LocalRotation.Angles().AsVector3(), target2Child.Transform.LocalRotation.Angles().AsVector3(),frac) * angMod.AsVector3();
            setChild.Transform.LocalRotation = new Angles(modifiedAngles.x,modifiedAngles.y,modifiedAngles.z);
			CopyTransformRecursiveLerp(target1Child, target2Child, setChild, posMod, angMod, frac);
        }
    }

	public class VRHandGrabber
	{
		public GameObject VRSpace {get;set;}
		
		public void DropItem(
			ref Sandbox.Physics.FixedJoint fixedJointRef,
			ref bool holdingRef,
			ref GameObject grabPointRef,
			ref HandPos currentHandPosRef,
			ref bool reHoldRef,
			GameObject handTarget,
			Vector3 handLocalPos,
			Rotation handLocalRot,
			GameObject handSkeleton)
		{
			bool knifeTrigger = false;
			if (grabPointRef.IsValid())
			{
				if (currentHandPosRef.Main && currentHandPosRef.item != null)
				{
					
					if(currentHandPosRef.Rigidbody.IsValid())
					{
						currentHandPosRef.Rigidbody.MotionEnabled = currentHandPosRef.item.MotionEnabled;
						if (currentHandPosRef.ParentTo)
							Game.ActiveScene.Components.GetInChildren<ChunkDealer>().PlaceInChunk(currentHandPosRef.Rigidbody.GameObject);
						if(currentHandPosRef.item.Controller != null)
							knifeTrigger = currentHandPosRef.item.Controller.Trigger > 0.75f;
					}
					currentHandPosRef.item.Controller = null;
					currentHandPosRef.item.mainHeld = false;
				}
				grabPointRef.Parent.Tags.Remove("grabbed");
			}
			if (currentHandPosRef.Connect && fixedJointRef.IsValid()) fixedJointRef.Remove();

			if (currentHandPosRef.item != null)
			{
				
				if (currentHandPosRef.item.HandsConnected <= 1 && currentHandPosRef.Rigidbody.IsValid())
					currentHandPosRef.Rigidbody.AngularDamping = 0;
				currentHandPosRef.item.HandsConnected--;
				if(currentHandPosRef.item.MotionEnabled) currentHandPosRef.item.Throw(knifeTrigger);
			}

			handTarget.Transform.LocalPosition = handLocalPos;
			handTarget.Transform.LocalRotation = handLocalRot;

			MakeAnimated(handSkeleton);

			fixedJointRef = null;
			holdingRef = false;
			grabPointRef = null;
			currentHandPosRef = null;
			reHoldRef = true;
		}

		public void PickupItem(
			GameObject closest,
			ref Sandbox.Physics.FixedJoint fixedJointRef,
			ref bool holdingRef,
			ref GameObject grabPointRef,
			ref HandPos currentHandPosRef,
			Rigidbody handPhys,
			VRController hand,
			GameObject handSkeleton)
		{
			HandPos handPos = closest.Components.Get<HandPos>();
			Sandbox.Physics.FixedJoint newFixedJoint = null;
			if(handPos.Rigidbody.IsValid()) 
			{
				handPos.Rigidbody.Enabled = true;
				Vector3 OriginPos = handPos.Rigidbody.Transform.Position;
				Rotation OriginRot = handPos.Rigidbody.Transform.Rotation;

				AlignByChild(handPos.Rigidbody.GameObject, handPos.GameObject, handPhys.GameObject);

				
				if (handPos.Connect)
				{
					var p1 = new PhysicsPoint(handPhys.PhysicsBody, handPhys.Transform.Position);
					var p2 = new PhysicsPoint(handPos.Rigidbody.PhysicsBody, closest.Parent.Transform.Position);
					newFixedJoint = PhysicsJoint.CreateFixed(p1, p2);
				}

				handPos.Rigidbody.Transform.Position = OriginPos;
				handPos.Rigidbody.Transform.Rotation = OriginRot;
			}

			Item item = handPos.item;
			if (item != null)
			{
				
				if(handPos.Rigidbody.IsValid()) handPos.Rigidbody.AngularDamping = item.AngularDrag;
				item.HandsConnected++;
				if(!item.HandPoss.Contains(handPos))
					item.HandPoss.Add(handPos);
				if(item.rigidbody.IsValid()) item.rigidbody.MotionEnabled = true;
				if (handPos.Connect && newFixedJoint.IsValid())
				{
					newFixedJoint.SpringAngular = new PhysicsSpring(handPos.AngularSpring.x, handPos.AngularSpring.y);
					newFixedJoint.SpringLinear = new PhysicsSpring(handPos.PositionSpring.x, handPos.PositionSpring.y);
				}

				if (handPos.Main)
				{
					item.Controller = hand;
					item.HandPos = handPos;
					item.mainHeld = true;
				}
			}
			else
			{
				newFixedJoint.SpringAngular = new PhysicsSpring(100, 10);
				newFixedJoint.SpringLinear = new PhysicsSpring(100, 10);
			}

			if (handPos.ParentTo) handPos.Rigidbody.GameObject.SetParent(VRSpace);
			closest.Parent.Tags.Add("grabbed");
			MakeProcedual(handSkeleton);

			fixedJointRef = newFixedJoint;
			holdingRef = true;
			grabPointRef = closest;
			currentHandPosRef = handPos;
			
		}

		public (Sandbox.Physics.FixedJoint fixedJoint, bool holding, GameObject grabPoint, HandPos currentHandPos, bool reHold)
		Grabber(
			GrabPointFinder pointFinder,
			Rigidbody handPhys,
			VRController hand,
			GameObject handSkeleton,
			Sandbox.Physics.FixedJoint fixedJointRef,
			bool holdingRef,
			GameObject grabPointRef,
			GameObject handTarget,
			Vector3 handLocalPos,
			Rotation handLocalRot,
			HandPos currentHandPosRef,
			bool reHoldRef,
			float lastTrigger)
		{
			if (holdingRef)
			{
				
				reHoldRef = true;
				if (hand.Grip < 0.5f || !grabPointRef.IsValid())
				{
					DropItem(ref fixedJointRef, ref holdingRef, ref grabPointRef, ref currentHandPosRef, ref reHoldRef, handTarget, handLocalPos, handLocalRot, handSkeleton);
					return (null, false, null, null, reHoldRef);
				}

				handTarget.Transform.Position = currentHandPosRef.wristObject.Transform.Position;
				handTarget.Transform.Rotation = currentHandPosRef.wristObject.Transform.Rotation;
				CopyTransformRecursive(currentHandPosRef.wristObject, handSkeleton, Vector3.One, new Angles(1, 1, 1));

				return (fixedJointRef, holdingRef, grabPointRef, currentHandPosRef, reHoldRef);
			}

			if (reHoldRef)
			{
				if (hand.Grip < 0.5f) reHoldRef = false;
				return (fixedJointRef, holdingRef, grabPointRef, currentHandPosRef, reHoldRef);
			}

			

			ItemStorer closestS = null;
			
			foreach (ItemStorer p in pointFinder.ItemStorers)
			{
				closestS = p;
				break;
			}

			if(closestS != null)
			{
				if (hand.Grip > 0.75f)
				{
					Item item = closestS.ReleaseItem();
					
					if(item != null)
					{
						HandPos handPos = item.HandPos.Tags.Contains(hand == Input.VR.LeftHand ? "left" : "right") ? item.HandPos : item.HandPos.OtherHand;
						PickupItem(handPos.GameObject, ref fixedJointRef, ref holdingRef, ref grabPointRef, ref currentHandPosRef, handPhys, hand, handSkeleton);
					}
					return (fixedJointRef, holdingRef, grabPointRef, currentHandPosRef, reHoldRef);
				}
				return (fixedJointRef, holdingRef, grabPointRef, currentHandPosRef, reHoldRef);
			}

			GameObject closest = null;
			float closestDis = 500;
			foreach (GameObject p in pointFinder.GrabbablePoints)
			{
				if (p.Tags.Contains("grabbed")) continue;
				float distance = Vector3.DistanceBetween(p.Parent.Transform.Position, pointFinder.searchPoint);
				if (distance > closestDis) continue;
				closest = p;
				closestDis = distance;
			}

			Interactable closestI = null;
			float closestIDis = 500;
			foreach (Interactable i in pointFinder.InteractablePoints)
			{
				if (!i.ShowWithoutMain) { if (!i.item.mainHeld) continue; }
				float distance = Vector3.DistanceBetween(i.Transform.Position, pointFinder.searchPoint);
				if (distance > closestIDis) continue;
				closestI = i;
				closestIDis = distance;
			}

			if (closestI != null)
			{
				Gizmo.Draw.Color = Color.Gray;
				Gizmo.Draw.SolidSphere(closestI.Transform.Position, 0.25f);
				if (hand.Trigger >= 0.75f && lastTrigger < 0.75f)
				{
					closestI.interacted = !closestI.interacted;
				}
			}

			if (closest == null) return (fixedJointRef, holdingRef, grabPointRef, currentHandPosRef, reHoldRef);
			Gizmo.Draw.Color = Color.White;
			Gizmo.Draw.IgnoreDepth = true;
			Gizmo.Draw.SolidSphere(closest.Parent.Transform.Position, 0.5f);

			if (hand.Grip > 0.75f)
			{
				PickupItem(closest, ref fixedJointRef, ref holdingRef, ref grabPointRef, ref currentHandPosRef, handPhys, hand, handSkeleton);
				return (fixedJointRef, holdingRef, grabPointRef, currentHandPosRef, reHoldRef);
			}

			return (fixedJointRef, holdingRef, grabPointRef, currentHandPosRef, reHoldRef);
		}
	}
}
