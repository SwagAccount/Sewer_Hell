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
	[Property] public List<string> Catagories {get;set;} = new List<string>();
	[Property] public int HandsConnected {get;set;}
	public VRController Controller {get;set;}
	[Property] public GameObject Functions {get;set;}
	[Property] public bool mainHeld {get;set;}
	[Property] public bool knifeThrow {get;set;}
	[Hide, Property] public Rigidbody rigidbody {get;set;}

	[Hide, Property] public List<HandPos> HandPoss {get;set;} = new List<HandPos>();
	[Hide, Property] public HandPos HandPos {get;set;}

	[Hide, Property] public PhysicsTracker physicsTracker {get;set;}

	[Hide, Property] public float TimeAlive {get;set;}
	[Property] public bool MotionEnabled {get;set;} = true;

	protected override void OnStart()
	{
		if(Renderer == null) Renderer = Components.GetInChildrenOrSelf<ModelRenderer>();
		physicsTracker = Components.GetOrCreate<PhysicsTracker>();
		rigidbody = Components.Get<Rigidbody>();
		TimeAlive = 0;
	}

	public bool InCatagory(List<string> CatagoriesRef)
	{
		if(CatagoriesRef == null) return true;
		if(CatagoriesRef.Count == 0) return true;
		if(Catagories == null) return false;
		foreach(string Catagory in CatagoriesRef)
		{
			foreach(string s in Catagories)
				if(s == Catagory) return true;
		}	
		return false;
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
	[Property,Hide] public bool lastInContainer {get;set;}
	[Property,Hide] public Vector3 containerPos {get;set;}
	[Property,Hide] public Rotation containerRot {get;set;}
	protected override void OnFixedUpdate()
	{
		TimeAlive += Time.Delta;
		Renderer.SceneObject.Attributes.Set("Condition", 1-Condition);
		if(Tags.Contains("container") && HandsConnected <= 0)
		{
			if(!Tags.Contains("contained")) Tags.Add("contained");
			if(!lastInContainer)
			{
				Log.Info("balls");
				containerPos = Transform.LocalPosition;
				containerRot = Transform.LocalRotation;
			}
			rigidbody.MotionEnabled = false;
			Transform.LocalPosition = containerPos;
			Transform.LocalRotation = containerRot;
		}
		else
		{
			if(Tags.Contains("contained")) Tags.Remove("contained");
		}
		lastInContainer = Tags.Contains("container") && HandsConnected <= 0;
		if(Functions==null) return;
		Functions.Enabled = mainHeld;
	}
}
