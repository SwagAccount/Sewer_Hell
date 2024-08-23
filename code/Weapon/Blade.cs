using System;
using Sandbox;
namespace trollface;
public sealed class Blade : Component
{
	[Property] public float damage {get;set;}
	[Property] public bool PlayerBlade {get;set;}
	[Property] public float baseAcceleration {get;set;}
	[Property] public float minAcceleration {get;set;} = 500f;
	[Property] public GameObject BladeEnd {get;set;}
	[Property] public GameObject User {get;set;}
	[Property] public float BladeRadius {get;set;} = 1f;
	[Property] public string[] ignoreTags {get;set;}
	[Property] public Item item {get;set;}
	[Property] public bool Stick {get;set;}

	[Property] public PhysicsTracker physicsTracker {get;set;}

	Vrmovement vrmovement;
	protected override void OnStart()
	{
		vrmovement = Scene.Components.GetInChildren<Vrmovement>();
		if(PlayerBlade)
			User = vrmovement.GameObject;
		if(physicsTracker == null) physicsTracker = Components.GetOrCreate<PhysicsTracker>();
		physicsTracker.TrackedObject = BladeEnd;
		
	}
	bool hit;
	public bool thrown;
	protected override void OnUpdate()
	{
		
		var ray = Scene.Trace.Ray(Transform.Position, BladeEnd.Transform.Position).IgnoreGameObjectHierarchy(User).Radius(BladeRadius).UseHitboxes().WithoutTags(ignoreTags).Run();
		if(ray.Hit && !hit)
		{
			HealthComponent healthComponent = ray.GameObject.Components.Get<HealthComponent>();
			if(Stick && thrown) stick(ray.GameObject);
			thrown = false;
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
				Vector3 hitAcc = Vector3.Zero;
				PhysicsTracker tracker = ray.GameObject.Components.GetInChildrenOrSelf<PhysicsTracker>();
				if(tracker != null) hitAcc = tracker.Acceleration;
				Log.Info(physicsTracker.Acceleration.Length);
				healthComponent.DoDamage(damage * ((physicsTracker.Acceleration.Length+hitAcc.Length)/baseAcceleration) * damageMult, User);
				Log.Info(damage * ((physicsTracker.Acceleration.Length+hitAcc.Length)/baseAcceleration) * damageMult);
				if(ray.Surface.Sounds.ImpactHard != null)
					Sound.Play(ray.Surface.Sounds.ImpactHard, ray.HitPosition);
				
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
