using System;
using Sandbox;
using trollface;

public sealed class LockBox : Component
{
	[Property] public List<KeyHole> KeyHoles {get;set;}
	[Property] public bool Flipped {get;set;}
	[Property] public bool On {get;set;}
	[Property] public float AngleThreshold {get;set;} = 10f;

	[Property] public SoundEvent ClickSound {get;set;} = ResourceLibrary.Get<SoundEvent>("sounds/switch.sound");

	protected override void OnStart()
	{

	}
	public class KeyHole
	{
		[KeyProperty] public Rotater Rotater {get;set;}
		[KeyProperty] public GameObject GrabPoint {get;set;}
		[KeyProperty] public Holster Holster {get;set;}
	}

	KeyHole UsedKeyhole = null;
	bool notExitedAngleThreshhold = false;
	protected override void OnUpdate()
	{
		if(UsedKeyhole == null)
		{
			foreach(KeyHole keyHole in KeyHoles)
			{
				keyHole.Holster.Enabled = true;
				if(keyHole.Holster.GameObject.Children.Count > 0)
				{
					UsedKeyhole = keyHole;
					keyHole.Rotater.Enabled = true;
					keyHole.Rotater.Rotated.Transform.LocalRotation = new Angles(On ? keyHole.Rotater.MaxAxis : keyHole.Rotater.MinAxis);
					notExitedAngleThreshhold = true;
					return;
				}
			}
			return;
		}

		foreach(KeyHole keyHole in KeyHoles)
		{
			keyHole.Holster.Enabled = keyHole == UsedKeyhole;
		}


		float currentAngle = UsedKeyhole.Rotater.Rotated.Transform.LocalRotation.Angles().AsVector3().Length; 
		if (MathF.Abs(currentAngle - UsedKeyhole.Rotater.MaxAxis.Length) < AngleThreshold)
        {
            if(On == Flipped)
                Sound.Play(ClickSound,Transform.Position);
			
            On = !Flipped;
        }
        else if (MathF.Abs(currentAngle - UsedKeyhole.Rotater.MinAxis.Length) < AngleThreshold)
        {
            if(On == !Flipped)
                Sound.Play(ClickSound,Transform.Position);

            On = Flipped;
        }

		if(UsedKeyhole.Holster.GameObject.Children.Count == 0)
		{
			UsedKeyhole.Holster.Tags.Remove("grabbed");
			UsedKeyhole.GrabPoint.Enabled = false;
			SnapKey(UsedKeyhole);
			UsedKeyhole = null;
			return;
		}

		if(notExitedAngleThreshhold)
		{
			Vector3 Angle = UsedKeyhole.Rotater.Rotated.Transform.LocalRotation.Angles().AsVector3();
			float maDis = Vector3.DistanceBetween(Angle,UsedKeyhole.Rotater.MaxAxis);
			float miDis = Vector3.DistanceBetween(Angle,UsedKeyhole.Rotater.MinAxis);
			notExitedAngleThreshhold = !(maDis > AngleThreshold && miDis > AngleThreshold);
		}
		else
		{
			UsedKeyhole.Rotater.Enabled = UsedKeyhole.Rotater.item.mainHeld;
		}

		UsedKeyhole.GrabPoint.Enabled = UsedKeyhole.Rotater.Enabled;

		if(UsedKeyhole.Rotater.Enabled)
		{
			if(!UsedKeyhole.Holster.Tags.Contains("grabbed")) UsedKeyhole.Holster.Tags.Add("grabbed");
		}
		else
			UsedKeyhole.Holster.Tags.Remove("grabbed");
	}

	void SnapKey(KeyHole keyHole)
	{
		Vector3 Angle = UsedKeyhole.Rotater.Rotated.Transform.LocalRotation.Angles().AsVector3();
		float maDis = Vector3.DistanceBetween(Angle, keyHole.Rotater.MaxAxis);
		float miDis = Vector3.DistanceBetween(Angle, keyHole.Rotater.MinAxis);
		keyHole.Rotater.Rotated.Transform.LocalRotation = new Angles(maDis < miDis ? keyHole.Rotater.MaxAxis : keyHole.Rotater.MinAxis); 
	}
}
