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
	public Angles ReturnRot {get;set;}
	public bool GotReturn {get;set;}
	protected override void OnStart()
	{
		if(GotReturn) return;
		GotReturn = true;
		ReturnPos = Body.Transform.LocalPosition;
		ReturnRot = Body.Transform.LocalRotation;
		RecoilTargetPos = ReturnPos;
		RecoilTargetRot = ReturnRot;
	}

	protected override void OnEnabled()
	{
		RecoilTargetPos = ReturnPos;
		RecoilTargetRot = ReturnRot;
		Body.Transform.LocalPosition = ReturnPos;
		Body.Transform.LocalRotation = ReturnRot;
	}

	public void ApplyRecoil()
	{
		RecoilTargetPos += RecoilPos[0] + (Vector3.Random * (RecoilPos[0]-RecoilPos[1]));
		RecoilTargetRot += RecoilRot[0] + (Vector3.Random * (RecoilRot[0]-RecoilRot[1]));
	}
	protected override void OnUpdate()
	{
		RecoilTargetPos = Vector3.Lerp(RecoilTargetPos,ReturnPos,PosReturnSpeed * Time.Delta);
		RecoilTargetRot = Angles.Lerp(RecoilTargetRot,ReturnRot,RotReturnSpeed * Time.Delta);

		Body.Transform.LocalPosition = Vector3.Lerp(Body.Transform.LocalPosition, RecoilTargetPos,PosRecoilSpeed * Time.Delta);
		Body.Transform.LocalRotation = Angles.Lerp(Body.Transform.LocalRotation, RecoilTargetRot,RotRecoilSpeed * Time.Delta);
	}
}
