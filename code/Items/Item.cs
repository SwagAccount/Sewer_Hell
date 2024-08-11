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
	[Property] public bool knifeThrow {get;set;}
	public Rigidbody rigidbody {get;set;}

	public PhysicsTracker physicsTracker {get;set;}

	protected override void OnStart()
	{
		if(Renderer == null) Renderer = Components.GetInChildrenOrSelf<ModelRenderer>();
		physicsTracker = Components.GetOrCreate<PhysicsTracker>();
		rigidbody = Components.Get<Rigidbody>();
	}

	public void Throw(bool knifeTrigger = false)
	{
		float velmult = 1;
		if(!knifeThrow || !knifeTrigger) rigidbody.AngularVelocity = physicsTracker.AngularVelocity;
		else
		{
			velmult = 4;
			Transform.Rotation = Rotation.LookAt(rigidbody.Velocity);
		}
		rigidbody.Velocity = physicsTracker.Velocity * velmult;
	}

	protected override void OnFixedUpdate()
	{
		//Condition = (MathF.Sin(Time.Now)+1)/2;
		Renderer.SceneObject.Attributes.Set("Condition", 1-Condition);	
		if(Functions==null) return;
		Functions.Enabled = mainHeld;
	}
}
