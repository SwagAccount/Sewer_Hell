using Sandbox.Utility;
using Sandbox.VR;

public sealed class Vrchecker : Component
{
	[Property] public TextRenderer Text {get;set;}
	TrackedObject f;
	protected override void OnStart()
	{
		if(Game.IsRunningInVR)
		{
			Scene.LoadFromFile("tftintro/intro.scene");
			return;
		}
		Text.Enabled = true;
	}
}
