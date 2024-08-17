using Sandbox;
namespace trollface;
public sealed class HandPos : Component
{
	[Property] public Rigidbody Rigidbody {get;set;}
	[Property] public HandPos OtherHand {get;set;}
	[Property] public bool Main {get;set;} = true;
	[Property] public bool ShowWithoutMain {get;set;} = true;
	[Property] public bool Connect {get;set;} = true;
	[Property] public bool ParentTo {get;set;} = true;
	[Property] public bool MirrorToOtherHand {get;set;}
	[Property] public Vector3 MirrorPosModifiers {get;set;} = new Vector3(-1,1,1);
	[Property] public Angles MirrorRotModifiers {get;set;} = new Angles(-1,-1,1);

	[Property] public Vector2 AngularSpring {get;set;} = new Vector2(100,10);
	[Property] public Vector2 PositionSpring {get;set;} = new Vector2(100,10);
	
	[Property] public GameObject wristObject;

	[Hide, Property]  public bool gotLocTrans {get;set;}
	[Hide, Property] public Rotation locRot {get;set;}
	[Hide, Property] public Vector3 locPos {get;set;}
	bool lastMirrorBool;
	public Item item;
	protected override void OnStart()
	{
		//Components.Get<SphereCollider>().Center = Transform.World.PointToLocal(GameObject.Transform.Parent.Transform.Position);
		if(!gotLocTrans)
		{
			gotLocTrans = true;
			locRot = Transform.LocalRotation; 
			locPos = Transform.LocalPosition; 
		}
		item = Rigidbody.Components.Get<Item>();

	}
	protected override void OnFixedUpdate()
	{
		//Rigidbody.Enabled = !Tags.Contains("container");
		Transform.LocalPosition = locPos;
		Transform.LocalRotation = locRot;
	}
	protected override void DrawGizmos()
	{
		if(MirrorToOtherHand && !lastMirrorBool)
		{
			HandsDealer.CopyTransformRecursive(wristObject, OtherHand.wristObject,MirrorPosModifiers, MirrorRotModifiers);
		}
		lastMirrorBool = MirrorToOtherHand;
	}
}
