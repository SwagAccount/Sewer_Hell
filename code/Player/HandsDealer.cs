using System;
using Sandbox;

public sealed class HandsDealer : Component
{
	[Property] public GameObject Head {get;set;}

	[Property] public SkinnedModelRenderer LeftArmRenderer {get;set;}
	[Property] public SkinnedModelRenderer RightArmRenderer {get;set;}
	[Property] public GameObject HandTargetL {get;set;}
	[Property] public GameObject HandTargetR {get;set;}

	[Property] public GameObject LArmAnchor {get;set;}
	[Property] public GameObject LArmParent {get;set;}
	[Property] public GameObject RArmAnchor {get;set;}
	[Property] public GameObject RArmParent {get;set;}
	[Property] public float HandSpeed {get;set;} = 10f;
	[Property] public float AnchorDistance {get;set;}

	protected override void OnStart()
	{
		
	}
	float leftGripAmount;
	float rightGripAmount;
	protected override void OnPreRender()
	{

		RightArmRenderer.SetIk("hand_right", HandTargetR.Transform.World);
		LeftArmRenderer.SetIk("hand_left", HandTargetL.Transform.World);

		leftGripAmount = MathX.Lerp(leftGripAmount,(Input.VR.LeftHand.Grip.Value+Input.VR.LeftHand.Trigger.Value)/2,Time.Delta*10);
		rightGripAmount = MathX.Lerp(rightGripAmount,(Input.VR.RightHand.Grip.Value+Input.VR.RightHand.Trigger.Value)/2,Time.Delta*10);
		
		LeftArmRenderer.Set("HoldAmount", leftGripAmount);
		RightArmRenderer.Set("HoldAmount", rightGripAmount);

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
