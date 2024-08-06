using Sandbox;

public sealed class SboxVrraycastStupidAndBroken : Component
{
	protected override void OnUpdate()
	{
		var ray = Scene.Trace.Ray(Transform.Position, Transform.Position + Transform.World.Forward * 100).UseHitboxes().Run();
		if(ray.Hit) Log.Info(ray.GameObject.Name);
	}
}
