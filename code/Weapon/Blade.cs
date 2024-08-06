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

	PhysicsTracker physicsTracker;
	protected override void OnStart()
	{
		physicsTracker = Components.GetOrCreate<PhysicsTracker>();
		physicsTracker.TrackedObject = BladeEnd;
	}
	bool hit;
	protected override void OnUpdate()
	{
		var ray = Scene.Trace.Ray(Transform.Position, BladeEnd.Transform.Position).IgnoreGameObjectHierarchy(User).Radius(BladeRadius).UseHitboxes().WithoutTags(ignoreTags).Run();
		if(ray.Hit && !hit)
		{
			HealthComponent healthComponent = ray.GameObject.Components.Get<HealthComponent>();
			Log.Info(ray.GameObject);
			if(healthComponent != null)
			{
				hit = true;
				healthComponent.Health -= damage * (physicsTracker.Acceleration.Length/baseAcceleration);
				
			}
		}
		else
		{
			if(!ray.Hit) hit = false;
		}
	}
}
