using System;
using Sandbox;

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

	[Property] public GameObject LArmAnchor {get;set;}
	[Property] public GameObject LArmParent {get;set;}
	[Property] public GameObject RArmAnchor {get;set;}
	[Property] public GameObject RArmParent {get;set;}
	[Property] public float HandSpeed {get;set;} = 10f;
	[Property] public float AnchorDistance {get;set;}

	float leftGripAmount;
	float rightGripAmount;

	protected override void OnStart()
	{
		positionFilterL = new OneEuroFilter<Vector3>(filterFrequency);
		positionFilterR = new OneEuroFilter<Vector3>(filterFrequency);
		rotationFilterL = new OneEuroFilter<Rotation>(filterFrequency);
		rotationFilterR = new OneEuroFilter<Rotation>(filterFrequency);
	}

	protected override void OnPreRender()
	{
		//    Hand Smoothing    \\

		// Left
		positionFilterL.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothL.Transform.Position = positionFilterL.Filter(HandRawL.Transform.Position);

		rotationFilterL.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothL.Transform.Rotation = rotationFilterL.Filter(HandRawL.Transform.Rotation);

		// Right
		positionFilterR.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothR.Transform.Position = positionFilterR.Filter(HandRawR.Transform.Position);

		rotationFilterR.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
		HandSmoothR.Transform.Rotation = rotationFilterR.Filter(HandRawR.Transform.Rotation);

		
		//    IK    \\
		RightArmRenderer.SetIk("hand_right", HandTargetR.Transform.World);
		LeftArmRenderer.SetIk("hand_left", HandTargetL.Transform.World);

		//    Animation    \\
		leftGripAmount = MathX.Lerp(leftGripAmount,(Input.VR.LeftHand.Grip.Value+Input.VR.LeftHand.Trigger.Value)/2,Time.Delta*10);
		rightGripAmount = MathX.Lerp(rightGripAmount,(Input.VR.RightHand.Grip.Value+Input.VR.RightHand.Trigger.Value)/2,Time.Delta*10);
		
		LeftArmRenderer.Set("HoldAmount", leftGripAmount);
		RightArmRenderer.Set("HoldAmount", rightGripAmount);

		//    Stretch Prevention    \\

		RArmParent.Transform.Rotation = Rotation.LookAt(HandTargetR.Transform.Position-RArmParent.Transform.Position);
		LArmParent.Transform.Rotation = Rotation.LookAt(HandTargetL.Transform.Position-LArmParent.Transform.Position);

		RightArmRenderer.GameObject.Parent.Transform.LocalPosition = Vector3.Zero.WithX(
			MathX.Clamp(Vector3.DistanceBetween(RArmAnchor.Transform.Position,HandTargetR.Transform.Position) - AnchorDistance,0,100)
		);
		LeftArmRenderer.GameObject.Parent.Transform.LocalPosition = Vector3.Zero.WithX(
			MathX.Clamp(Vector3.DistanceBetween(LArmAnchor.Transform.Position,HandTargetL.Transform.Position) - AnchorDistance,0,100)
		);

	}
}
