using Sandbox;
using Sandbox.VR;
namespace trollface;
public sealed class Item : Component
{
	[Property] public string ItemName {get;set;}
	[Property] public int HandsConnected {get;set;}
	[Property] public VRController Controller {get;set;}
	[Property] public GameObject Functions {get;set;}
	[Property] public Vector2 AngularSpring {get;set;} = new Vector2(100,10);
	[Property] public Vector2 PositionSpring {get;set;} = new Vector2(100,10);
	[Property] public bool mainHeld {get;set;}

	protected override void OnFixedUpdate()
	{
		if(Functions==null) return;
		Functions.Enabled = mainHeld;
	}
}
