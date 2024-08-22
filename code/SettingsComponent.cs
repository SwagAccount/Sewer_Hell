using Sandbox;
using trollface;

public sealed class SettingsComponent : Component
{
	[Property]
	public GameObject Space {get;set;}

	public bool InSettings;

	CameraComponent mainCam;

	protected override void OnStart()
	{
		mainCam = Scene.Components.GetInChildren<Vrmovement>().Camera.Components.Get<CameraComponent>();
	}
	protected override void OnUpdate()
	{
		if(Input.VR.LeftHand.JoystickPress && Input.VR.LeftHand.JoystickPress && !InSettings)
		{
			GoIntoSettings();
		}
	}

	public void GoIntoSettings()
	{
		InSettings = true;
		Space.Enabled = true;
		mainCam.Enabled = false;
		Scene.TimeScale = 0;
	}

	public void ExitSettings()
	{
		InSettings = false;
		Space.Enabled = false;
		mainCam.Enabled = true;
		Scene.TimeScale = 1;
	}
}
