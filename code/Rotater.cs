using System;
using Sandbox;
using trollface;

public sealed class Rotater : Component
{
	[Property] public GameObject Rotated { get; set; }
	[Property] public Item item { get; set; }
	[Property] public Vector3 MinAxis { get; set; }
	[Property] public Vector3 MaxAxis { get; set; }
	[Property] public float RotateSpeed { get; set; } = 10;
	[Property] public int Increments { get; set; } = 0;
	Vrmovement vrmovement;

	Vector3 ActualMinAxis;
	Vector3 ActualMaxAxis;

	protected override void OnStart()
	{
		ActualMinAxis = MinAxis;
		ActualMaxAxis = MaxAxis;
		vrmovement = Scene.Components.GetInChildren<Vrmovement>();
	}

	public int GetValue()
	{
		Angles localAngles = Rotated.Transform.LocalRotation.Angles();

		float proportionPitch = (localAngles.pitch - ActualMinAxis.x) / (ActualMaxAxis.x - ActualMinAxis.x);
		float proportionYaw = (localAngles.yaw - ActualMinAxis.y) / (ActualMaxAxis.y - ActualMinAxis.y);
		float proportionRoll = (localAngles.roll - ActualMinAxis.z) / (ActualMaxAxis.z - ActualMinAxis.z);

		float averageProportion = (proportionPitch + proportionYaw + proportionRoll) / 3.0f;

		return Math.Clamp((int)MathF.Round(averageProportion * Increments), 0, Increments);
	}

	public void SetValue(int value)
	{
		value = Math.Clamp(value, 0, Increments);

		float proportion = value / (float)Increments;

		Angles targetAngles = new Angles(
			MathX.Lerp(ActualMinAxis.x, ActualMaxAxis.x, proportion),
			MathX.Lerp(ActualMinAxis.y, ActualMaxAxis.y, proportion),
			MathX.Lerp(ActualMinAxis.z, ActualMaxAxis.z, proportion)
		);

		Rotated.Transform.LocalRotation = Rotation.From(targetAngles);
	}

	protected override void OnUpdate()
	{
		ActualMaxAxis = Vector3.Lerp(ActualMaxAxis, MaxAxis, RotateSpeed * Time.Delta);
		ActualMinAxis = Vector3.Lerp(ActualMinAxis, MinAxis, RotateSpeed * Time.Delta);

		if (item.mainHeld && item.HandPos.IsValid())
		{
			Vector3 handPosition = (item.HandPos.Tags.Contains("right") ? vrmovement.RawRightHand : vrmovement.RawLeftHand).Transform.Position;
			Vector3 direction = (handPosition - Rotated.Transform.Position).Normal;
			Rotated.Transform.Rotation = Rotation.LookAt(direction);
		}

		Angles localAngles = Rotated.Transform.LocalRotation.Angles();
		localAngles = new Angles(
			MathX.Clamp(localAngles.pitch, ActualMinAxis.x, ActualMaxAxis.x),
			MathX.Clamp(localAngles.yaw, ActualMinAxis.y, ActualMaxAxis.y),
			MathX.Clamp(localAngles.roll, ActualMinAxis.z, ActualMaxAxis.z)
		);

		if (Increments > 0)
		{
			float snapX = (ActualMaxAxis.x - ActualMinAxis.x) / Increments;
			float snapY = (ActualMaxAxis.y - ActualMinAxis.y) / Increments;
			float snapZ = (ActualMaxAxis.z - ActualMinAxis.z) / Increments;

			localAngles.pitch = MathF.Round(localAngles.pitch / snapX) * snapX;
			localAngles.yaw = MathF.Round(localAngles.yaw / snapY) * snapY;
			localAngles.roll = MathF.Round(localAngles.roll / snapZ) * snapZ;
		}

		Rotated.Transform.LocalRotation = Rotation.From(localAngles);
	}
}
