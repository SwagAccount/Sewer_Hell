using System;
using Sandbox;

namespace trollface;

public sealed class Slider : Component
{
	[Property] public GameObject Slid { get; set; }
	[Property] public Item item { get; set; }
	[Property] public Vector3 MinPos { get; set; }
	[Property] public Vector3 MaxPos { get; set; }
	[Property] public float RotateSpeed { get; set; } = 10;
	[Property] public int Increments { get; set; } = 0;
	Vrmovement vrmovement;

	Vector3 ActualMinPos;
	Vector3 ActualMaxPos;

	protected override void OnStart()
	{
		ActualMinPos = MinPos;
		ActualMaxPos = MaxPos;
		vrmovement = Scene.Components.GetInChildren<Vrmovement>();
	}

	public int GetValue()
	{
		Vector3 direction = MaxPos - MinPos;

		Vector3 vectorToTarget = Slid.Transform.LocalPosition - MinPos;

		float distanceAlongDirection = Vector3.Dot(vectorToTarget, direction.Normal);

		float fractionAcrossDirection = 0;
		
		if(distanceAlongDirection > 0)
			fractionAcrossDirection = distanceAlongDirection/Vector3.DistanceBetween(MaxPos, MinPos);

		return (int)MathF.Round(fractionAcrossDirection*Increments);
	}

	public void SetValue(int value)
	{
		
		value = Math.Clamp(value, 0, Increments);

		float proportion = value / (float)Increments;
		
		Vector3 targetPosition = new Vector3(
			MathX.Lerp(MinPos.x, MaxPos.x, proportion),
			MathX.Lerp(MinPos.y, MaxPos.y, proportion),
			MathX.Lerp(MinPos.z, MaxPos.z, proportion)
		);

		Slid.Transform.LocalPosition = targetPosition;
	}

	public bool incrementUpdated;
	protected override void OnUpdate()
	{
		ActualMaxPos = Vector3.Lerp(ActualMaxPos, MaxPos, RotateSpeed * Time.Delta);
		ActualMinPos = Vector3.Lerp(ActualMinPos, MinPos, RotateSpeed * Time.Delta);
		Vector3 prevPos = Slid.Transform.LocalPosition;

		if (item.mainHeld && item.HandPos.IsValid())
			Slid.Transform.Position = (item.HandPos.Tags.Contains("right") ? vrmovement.RawRightHand : vrmovement.RawLeftHand).Transform.Position;
		
		Vector3 localPos = Slid.Transform.LocalPosition;
		localPos = new Vector3(
			MathX.Clamp(localPos.x, ActualMinPos.x, ActualMaxPos.x),
			MathX.Clamp(localPos.y, ActualMinPos.y, ActualMaxPos.y),
			MathX.Clamp(localPos.z, ActualMinPos.z, ActualMaxPos.z)
		);

		var snappedLocal = localPos;

		if (Increments > 0)
		{
			incrementUpdated = false;
			float snapX = (ActualMaxPos.x - ActualMinPos.x) / Increments;
			float snapY = (ActualMaxPos.y - ActualMinPos.y) / Increments;
			float snapZ = (ActualMaxPos.z - ActualMinPos.z) / Increments;

			if(snapX != 0) localPos.x = MathF.Round(localPos.x / snapX) * snapX;
			if(snapY != 0) localPos.y = MathF.Round(localPos.y / snapY) * snapY;
			if(snapZ != 0) localPos.z = MathF.Round(localPos.z / snapZ) * snapZ;
			if(MathF.Round(prevPos.x*10) != MathF.Round(localPos.x*10) || MathF.Round(prevPos.y*10) != MathF.Round(localPos.y*10) || MathF.Round(prevPos.z*10) != MathF.Round(localPos.z*10))
				incrementUpdated = true;
		
		}
		Slid.Transform.LocalPosition = localPos;
	}
}
