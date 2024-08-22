using System;
using Sandbox;
using trollface;

public sealed class BulletProjectile : Component
{
	public Bullet bullet;
	public GameObject Firerer;
    private Rigidbody rB;
	Vector3 lastPos;

	//List<Vector3> poss;
	protected override void OnStart()
    {
		//poss = new List<Vector3> { Transform.Position };
		rB = Components.GetOrCreate<Rigidbody>();
        lastPos = Transform.Position;
    }
	bool hitSomething;
	protected override void OnUpdate()
	{
		//poss.Add(Transform.Position);
		if(hitSomething) return;
		
        var ray = Scene.Trace.Ray(lastPos,Transform.Position).Radius(bullet.Diameter/2).UseHitboxes().IgnoreGameObjectHierarchy(Firerer).Run();

		if(ray.Hit)
		{
			
			hitSomething = true;

			
			HealthComponent healthComponent = ray.GameObject.Components.Get<HealthComponent>();
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
				Log.Info(MathF.Pow(bullet.Grain,2f)*(rB.Velocity.Length*12)/(700000*MathF.Pow(bullet.Diameter,2f))*0.0006f*damageMult);
				float damage = MathF.Pow(bullet.Grain,2f)*(rB.Velocity.Length*12)/(700000*MathF.Pow(bullet.Diameter,2f))*0.0012f*damageMult;
				
				healthComponent.Health -= damage;
			}
			rB.MotionEnabled = false;
			Log.Info(ray.GameObject);
			GameObject.Destroy();
		}
        lastPos = Transform.Position;
	}
	/*
	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Red;
        for (int i = 0; i < poss.Count - 1; i++)
        {
            Gizmo.Draw.Line(Transform.World.PointToLocal(poss[i]), Transform.World.PointToLocal(poss[i + 1]));
        }
	}*/
}
