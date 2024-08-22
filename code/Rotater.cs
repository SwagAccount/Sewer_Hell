using Sandbox;
using trollface;

public sealed class Rotater : Component
{
	[Property] public GameObject Rotated {get;set;}
	[Property] public Item item {get;set;}
	[Property] public Vector3 MinAxis {get;set;}
	[Property] public Vector3 MaxAxis {get;set;}
	[Property] public float RotateSpeed {get;set;} = 10;
	Vrmovement vrmovement;
	
	Vector3 ActualMinAxis;
	Vector3 ActualMaxAxis;
	protected override void OnStart()
	{
		ActualMinAxis = MinAxis;
		ActualMaxAxis = MaxAxis;
		vrmovement = Scene.Components.GetInChildren<Vrmovement>();
	}
	protected override void OnUpdate()
	{
		ActualMaxAxis = Vector3.Lerp(ActualMaxAxis, MaxAxis, RotateSpeed * Time.Delta);
		ActualMinAxis = Vector3.Lerp(ActualMinAxis, MinAxis, RotateSpeed * Time.Delta);

		if(item.mainHeld && item.HandPos.IsValid()) Rotated.Transform.Rotation = Rotation.LookAt(((item.HandPos.Tags.Contains("right") ? vrmovement.RawRightHand: vrmovement.RawLeftHand).Transform.Position - Rotated.Transform.Position).Normal);
		Rotated.Transform.LocalRotation = new Angles(
			MathX.Clamp(Rotated.Transform.LocalRotation.Pitch(), ActualMinAxis.x, ActualMaxAxis.x),
			MathX.Clamp(Rotated.Transform.LocalRotation.Yaw(), ActualMinAxis.y, ActualMaxAxis.y),
			MathX.Clamp(Rotated.Transform.LocalRotation.Roll(), ActualMinAxis.z, ActualMaxAxis.z)
		);
	}
}
