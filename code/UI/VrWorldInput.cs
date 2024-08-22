using Sandbox;

public sealed class VrWorldInput : Component
{
	private Sandbox.UI.WorldInput worldInput = new();

	public enum Hands
	{
		Right,
		Left
	}

	protected override void OnUpdate()
	{
		worldInput.Enabled = true;
		worldInput.Ray = new Ray( Transform.World.Position, Transform.World.Position + Transform.World.Forward * 100000f );
		//Log.Info(worldInput.Hovered);
		worldInput.MouseLeftPressed = Input.VR.RightHand.Trigger >= 0.75f;
	}


}
