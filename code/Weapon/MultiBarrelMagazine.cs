using System;
using Sandbox;

namespace trollface;

public sealed class MultiBarrelMagazine : MagazineBase
{
	[Property] public float OpenSpeed {get;set;} = 100f;
	[Property] public float FlickSpeed {get;set;}
	[Property] public Angles FlickRotation {get;set;}
	[Property] public List<RotatedOpen> RotatedOpened {get;set;}
	[Property] public GameObject SoundPoint {get;set;}
	[Property] public SoundEvent OpenSound {get;set;}
	[Property] public SoundEvent CloseSound {get;set;}
	[Property] public SoundEvent EjectSound {get;set;}
	[Property] public GameObject Case {get;set;}
	[Property] public GameObject DropAxisRef {get;set;}

	public class RotatedOpen
	{
		[KeyProperty] public GameObject Rotated {get;set;}
		public Rotation CloseRot {get;set;}
		public Rotation OpenRot {get;set;}
		public float StartRotPoint {get;set;}
		public float EndRotPoint {get;set;} = 1;
		public bool Repeat {get;set;}
	}
	public float OpenAmount {get;set;}
	Item item;
	MultiBarrelTrigger multiBarrelTrigger;
	public bool open {get;set;}
	protected override void OnStart()
	{
		multiBarrelTrigger = Components.Get<MultiBarrelTrigger>();
		item = GameObject.Parent.Components.Get<Item>();
		CantLoad = true;
	}
	public int currentBarrel {get;set;}
	public void LoadBarrel(BarrelBase barrel)
	{
		barrel.BarrelContent = Contents[currentBarrel];
	}
	public void Shoot()
	{
		
		if(Contents[currentBarrel] != -1) Contents[currentBarrel] = -2;
		
		currentBarrel++;
		if(currentBarrel >= Contents.Count) currentBarrel = 0;

		Log.Info(currentBarrel);
	}

	protected override void OnUpdate()
	{
		if(!item.mainHeld) return;

		if(open) isOpen();
		else isClosed();
		
		foreach(RotatedOpen rotatedOpen in RotatedOpened)
		{
			float frac = (OpenAmount- rotatedOpen.StartRotPoint) / (rotatedOpen.EndRotPoint - rotatedOpen.StartRotPoint);
			if(frac >= 0 || !rotatedOpen.Repeat)
				rotatedOpen.Rotated.Transform.LocalRotation = Rotation.Lerp(rotatedOpen.CloseRot, rotatedOpen.OpenRot, MathX.Clamp(frac,0,1));
		}

		OpenAmount = MathX.Lerp(OpenAmount, open ? 1 : 0, Time.Delta * OpenSpeed);
	}
	bool Dropped;
	Rotation lastRot;

	void isOpen()
	{
		if(OpenAmount.AlmostEqual(1,0.01f) && Contents.Contains(-2) && !Dropped && CoverFinder.GetAngleBetweenDirections(Vector3.Up,DropAxisRef.Transform.World.Down) < 45)
		{
			DropCases(EjectSound, Case);
			UpdateVisuals();
			Dropped = true;
			CantLoad = false;
		}
		if(item.Controller.ButtonB.IsPressed && !item.Controller.ButtonB.WasPressed)
		{
			open = false;
			Sound.Play(CloseSound, SoundPoint.Transform.Position);
		}
		if(((((item.Transform.World.RotationToLocal(item.Controller.Transform.Rotation) - lastRot) * Time.Delta).Angles().AsVector3() * FlickRotation).Length * 10) > FlickSpeed && OpenAmount.AlmostEqual(1,0.01f))
		{
			open = false;
			Sound.Play(CloseSound, SoundPoint.Transform.Position);
		}

		lastRot = item.Transform.World.RotationToLocal(item.Controller.Transform.Rotation);
	}
	void isClosed()
	{
		if(item.Controller.ButtonB.IsPressed && !item.Controller.ButtonB.WasPressed && multiBarrelTrigger.pullBack == 0)
		{
			Sound.Play(OpenSound, SoundPoint.Transform.Position);
			open = true;
			Dropped = false;
		}
	}

}
