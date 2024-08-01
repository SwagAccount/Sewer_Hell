using System;
using Sandbox;
namespace trollface;
public sealed class RevolverCylinder : MagazineBase
{
	[Property] public GameObject CylinderBone {get;set;}
	[Property] public GameObject CylinderPivotBone {get;set;}
	[Property] public Angles PivotClosedRotation {get;set;}
	[Property] public Angles PivotOpenRotation {get;set;}
	[Property] public float CylinderPivotSpeed {get;set;} = 100f;
	[Property] public Angles BaseRotation {get;set;}
	[Property] public Angles RotateDirection {get;set;}
	[Property] public float RotateAmount {get;set;}
	[Property] public float FlickSpeed {get;set;}
	[Property] public Vector3 FlickRotation {get;set;}
	[Property] public float OpenAmount {get;set;}
	[Property] public GameObject[] CylinderBulletVisuals {get;set;}
	[Property] public GameObject Case {get;set;}

	public int LoadIndex {get;set;} = 0;

	public bool open;

	Item item;

	public void Shoot()
	{
		if(Contents[correctedLoadIndex()] != -1) Contents[correctedLoadIndex()] = -2;
	}

	protected override void OnStart()
	{
		item = GameObject.Parent.Components.Get<Item>();
		UpdateVisuals();
	}

	protected override void OnUpdate()
	{
		CylinderBone.Transform.LocalRotation = BaseRotation + Angles.Lerp(RotateDirection * (360/Contents.Count) * LoadIndex, RotateDirection * (360/Contents.Count) * (LoadIndex+1), RotateAmount);

		if(open) CylinderOpen();
		else CylinderClosed();

		CylinderPivotBone.Transform.LocalRotation = Angles.Lerp(PivotClosedRotation, PivotOpenRotation, OpenAmount);

		OpenAmount = MathX.Lerp(OpenAmount, open? 1:0,CylinderPivotSpeed*Time.Delta);

	}

	Rotation lastRot;
	bool Dropped;
	void CylinderOpen()
	{
		if(OpenAmount.AlmostEqual(1,0.01f) && Contents.Contains(-2) && !Dropped)
		{
			DropCases();
			UpdateVisuals();
			Dropped = true;
			CantLoad = false;
		}
		if((
			item.Controller.ButtonB.IsPressed && !item.Controller.ButtonB.WasPressed) 
			|| 
			((((item.Transform.World.RotationToLocal(item.Controller.Transform.Rotation) - lastRot) * Time.Delta).Angles().AsVector3() * FlickRotation).Length * 10) > FlickSpeed
			
			) open = false;

		lastRot = item.Transform.World.RotationToLocal(item.Controller.Transform.Rotation);

	}
	void CylinderClosed()
	{
		CantLoad = true;
		Dropped = false;
		if(item.Controller.ButtonB.IsPressed && !item.Controller.ButtonB.WasPressed) open = true;
		Log.Info(item.Controller.ButtonB.IsPressed);
	}

	void DropCases()
	{
		for(int i = 0; i < CylinderBulletVisuals.Count(); i++)
		{
			if(Contents[i] != -2) continue;
			
			GameObject newCase = Case.Clone();
			newCase.SetParent(CylinderBulletVisuals[i].Parent);
			newCase.Transform.Position = CylinderBulletVisuals[i].Transform.Position;
			newCase.Transform.Rotation = CylinderBulletVisuals[i].Transform.Rotation;
			Contents[i] = -1;
		}
	}

	public override void LoadBarrel()
	{
		LoadIndex++;
		Barrel.BarrelContent = Math.Clamp(Contents[correctedLoadIndex()],-1,100);
	}

	public override void UpdateVisuals()
	{
		for(int i = 0; i < CylinderBulletVisuals.Count(); i++)
		{
			CylinderBulletVisuals[i].Enabled = Contents[i] != -1;
		}
	}

	int correctedLoadIndex()
	{
		if (Contents.Count == 0) return 0; 

    	int correctedIndex = LoadIndex % Contents.Count;
    	if (correctedIndex < 0) correctedIndex += Contents.Count;

    	return correctedIndex;
	}
}