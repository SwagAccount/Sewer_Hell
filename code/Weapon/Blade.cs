using System;
using Sandbox;
namespace trollface;
public sealed class Blade : Component
{
	[Property] public float damage {get;set;}
	[Property] public float baseAcceleration {get;set;}
	[Property] public float minAcceleration {get;set;} = 500f;
	[Property] public GameObject BladeEnd {get;set;}
	[Property] public GameObject User {get;set;}
	[Property] public float BladeRadius {get;set;} = 1f;
	[Property] public string[] ignoreTags {get;set;}
	[Property] public Item item {get;set;}
	[Property] public bool Stick {get;set;}

	[Property] public PhysicsTracker physicsTracker {get;set;}
	protected override void OnStart()
	{
		if(physicsTracker == null) physicsTracker = Components.GetOrCreate<PhysicsTracker>();
		physicsTracker.TrackedObject = BladeEnd;
		
	}
	bool hit;
	protected override void OnUpdate()
	{
		var ray = Scene.Trace.Ray(Transform.Position, BladeEnd.Transform.Position).IgnoreGameObjectHierarchy(User).Radius(BladeRadius).UseHitboxes().WithoutTags(ignoreTags).Run();
		if(ray.Hit && !hit)
		{
			HealthComponent healthComponent = ray.GameObject.Components.Get<HealthComponent>();
			if(Stick) stick(ray.GameObject);
			if(healthComponent != null)
			{
				float damageMult = 1;
				if(ray.Hitbox != null)
				{
					IEnumerable<string> tags = ray.Hitbox.Tags.TryGetAll();
					
					foreach(string s in tags)
					{
						if(float.TryParse(s, out damageMult)) break;
					}
				}
				hit = true;
				healthComponent.Health -= damage * (physicsTracker.Acceleration.Length/baseAcceleration) * damageMult;
				
			}
		}
		else
		{
			if(!ray.Hit) hit = false;
		}
	}

	void stick(GameObject g)
	{
		if(item.mainHeld) return;
		item.rigidbody.MotionEnabled = false;
		item.GameObject.Transform.Position += item.GameObject.Transform.World.Forward * Vector3.DistanceBetween(Transform.Position, BladeEnd.Transform.Position)*0.75f;
		item.GameObject.SetParent(g);
	}
}
