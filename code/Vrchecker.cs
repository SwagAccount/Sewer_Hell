using Sandbox.Utility;
using Sandbox.VR;

public sealed class Vrchecker : Component
{
	[Property] public GameObject Text {get;set;}
	TrackedObject f;
	protected override void OnStart()
	{
		if(Game.IsRunningInVR)
		{
			Scene.LoadFromFile("scenes/menu.scene");
			return;
		}
		Text.Enabled = true;

		
	}
}
