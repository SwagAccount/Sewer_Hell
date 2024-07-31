using Sandbox;

public sealed class HandPos : Component
{
	[Property] public HandPos OtherHand {get;set;}
	[Property] public bool MirrorToOtherHand {get;set;}
	[Property] public Vector3 MirrorPosModifiers {get;set;} = Vector3.One;
	[Property] public Angles MirrorRotModifiers {get;set;} = new Angles(-1,-1,1);
	[Property] public GameObject wristObject;
	bool lastMirrorBool;
	protected override void DrawGizmos()
	{
		if(MirrorToOtherHand && !lastMirrorBool)
		{
			HandsDealer.CopyTransformRecursive(wristObject,OtherHand.wristObject,MirrorPosModifiers,MirrorRotModifiers);
		}
		lastMirrorBool = MirrorToOtherHand;
	}
}
