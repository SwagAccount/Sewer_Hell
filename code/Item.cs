using Sandbox;
using Sandbox.VR;
namespace trollface;
public sealed class Item : Component
{
	[Property] public string ItemName {get;set;}
	[Property] public VRController Controller {get;set;}
	[Property] public GameObject Functions {get;set;}
	[Property] public bool held {get;set;}

	protected override void OnFixedUpdate()
	{
		if(Functions==null) return;
		Functions.Enabled = held;
	}
}
