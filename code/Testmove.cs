using Sandbox;

public sealed class Testmove : Component
{
	protected override void OnUpdate()
	{
		Transform.Position += Transform.World.Forward * 10 * Time.Delta;
	}
}
