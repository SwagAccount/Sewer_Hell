using Sandbox;
using System;
using trollface;

public sealed class DisplayHealth : Component
{
	HealthComponent healthComponent;
	GameObject target;
	[Property] public Vector3 DisplayPos {get;set;}
	protected override void OnStart()
	{
		healthComponent = Components.Get<HealthComponent>();
		target = Scene.Components.GetInChildren<Vrmovement>().Camera.GameObject;
	}
	protected override void OnUpdate()
	{
		Gizmo.Draw.WorldText($"{MathF.Round(healthComponent.Health)}", new Transform(Transform.World.PointToWorld(DisplayPos), 
		Rotation.LookAt(Rotation.LookAt(target.Transform.Position - Transform.World.PointToWorld(DisplayPos)).Up)* new Angles(0,90,180), 0.1f
		),"Roboto", 60);
	}
}
