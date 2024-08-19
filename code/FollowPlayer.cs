using Sandbox;
using trollface;

public sealed class FollowPlayer : Component
{
	GameObject player;

	protected override void OnStart()
	{
		player = Scene.Components.GetInChildren<Vrmovement>().characterController.GameObject;
	}
	protected override void OnUpdate()
	{
		Transform.Position = player.Transform.Position;
	}
}
