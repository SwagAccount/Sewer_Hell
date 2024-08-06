using System;
using Sandbox;
namespace trollface;
public sealed class Recoil : Component
{
	[Property] public GameObject Body {get;set;}
	[Property] public Vector3[] RecoilPos {get;set;}
	[Property] public Angles[] RecoilRot {get;set;}
	[Property] public float PosRecoilSpeed {get;set;}
	[Property] public float RotRecoilSpeed {get;set;}
	[Property] public float PosReturnSpeed {get;set;}
	[Property] public float RotReturnSpeed {get;set;}

	public Vector3 RecoilTargetPos {get;set;}
	public Angles RecoilTargetRot {get;set;}
	public Vector3 ReturnPos {get;set;}
	[Property] public Angles ReturnRot {get;set;}
	public bool GotReturn {get;set;}
	
	Item item;
	protected override void OnStart()
	{
		item = GameObject.Parent.Components.Get<Item>();
		if(GotReturn) return;
		
		GotReturn = true;
		ReturnPos = Body.Transform.LocalPosition;
		if(ReturnRot.AsVector3().Length == 0) ReturnRot = Body.Transform.LocalRotation;
		RecoilTargetPos = ReturnPos;
		RecoilTargetRot = ReturnRot;
	}

	protected override void OnEnabled()
	{
		if(!GotReturn) return;
		RecoilTargetPos = ReturnPos;
		RecoilTargetRot = ReturnRot;
		Body.Transform.LocalPosition = ReturnPos;
		Body.Transform.LocalRotation = ReturnRot;
	}

	public void ApplyRecoil()
	{
		RecoilTargetPos += RecoilPos[0] + (Vector3.Random * (RecoilPos[0]-RecoilPos[1])/(item.HandsConnected * 1.5f));
		RecoilTargetRot += RecoilRot[0] + (Vector3.Random * (RecoilRot[0]-RecoilRot[1])/(item.HandsConnected * 1.5f));
	}
	protected override void OnUpdate()
	{
		RecoilTargetPos = Vector3.Lerp(RecoilTargetPos,ReturnPos,PosReturnSpeed * Time.Delta * item.HandsConnected * 1.5f);
		RecoilTargetRot = Angles.Lerp(RecoilTargetRot,ReturnRot,RotReturnSpeed * Time.Delta * item.HandsConnected * 1.5f);

		Body.Transform.LocalPosition = Vector3.Lerp(Body.Transform.LocalPosition, RecoilTargetPos,PosRecoilSpeed * Time.Delta);
		Body.Transform.LocalRotation = Angles.Lerp(Body.Transform.LocalRotation, RecoilTargetRot,RotRecoilSpeed * Time.Delta);
	}
}
