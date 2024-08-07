using Sandbox;
using Sandbox.VR;
namespace trollface;
public sealed class Item : Component
{
	[Property] public float AngularDrag {get;set;} = 1000000;
	[Property] public string ItemName {get;set;}
	[Property] public int HandsConnected {get;set;}
	[Property] public VRController Controller {get;set;}
	[Property] public GameObject Functions {get;set;}
	[Property] public bool mainHeld {get;set;}

	public Rigidbody rigidbody {get;set;}

	protected override void OnStart()
	{
		rigidbody = Components.Get<Rigidbody>();
	}
	protected override void OnFixedUpdate()
	{
		if(Functions==null) return;
		Functions.Enabled = mainHeld;
	}
}
