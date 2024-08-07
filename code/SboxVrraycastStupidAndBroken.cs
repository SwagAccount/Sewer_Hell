using Sandbox;

public sealed class SboxVrraycastStupidAndBroken : Component
{
	protected override void OnUpdate()
	{
		var ray = Scene.Trace.Ray(Transform.Position, Transform.Position + Transform.World.Forward * 10000).UseHitboxes().Run();
		Gizmo.Draw.Line(Transform.Position, Transform.Position + Transform.World.Forward * 10000);
		if(ray.Hit) Log.Info(ray.GameObject.Name);
		//Transform.Rotation = Transform.Rotation * new Angles(0,Time.Delta*50,0).ToRotation();
	}
}
