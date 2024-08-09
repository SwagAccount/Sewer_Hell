using Sandbox;
using System;
using Sandbox.VR;
namespace trollface;
public sealed class Item : Component
{
	[Property] public float AngularDrag {get;set;} = 1000000;
	[Property] public float Condition {get;set;} = 1;
	[Property] public ModelRenderer Renderer {get;set;}
	[Property] public string ItemName {get;set;}
	[Property] public int HandsConnected {get;set;}
	[Property] public VRController Controller {get;set;}
	[Property] public GameObject Functions {get;set;}
	[Property] public bool mainHeld {get;set;}

	public Rigidbody rigidbody {get;set;}

	protected override void OnStart()
	{
		if(Renderer == null) Renderer = Components.GetInChildrenOrSelf<ModelRenderer>();
		rigidbody = Components.Get<Rigidbody>();
	}
	protected override void OnFixedUpdate()
	{
		//Condition = (MathF.Sin(Time.Now)+1)/2;
		Renderer.SceneObject.Attributes.Set("Condition", 1-Condition);	
		if(Functions==null) return;
		Functions.Enabled = mainHeld;
	}
}
