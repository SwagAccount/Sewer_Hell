using Sandbox;
using trollface;

public sealed class SaveAndLeave : Component
{
	[Property] public Lever Save {get;set;}
	[Property] public Lever Leave {get;set;}
	public bool lastSave;
	GameManager gameManager;
	protected override void OnStart()
	{
		gameManager = Scene.Components.GetInChildren<GameManager>();
	}
	protected override void OnUpdate()
	{
		if(Save.On && !lastSave)
		{
			gameManager.Save();
		}
		lastSave = Save.On;


		if(Leave.On)
		{
			Scene.LoadFromFile("scenes/menu.scene");
		}
	}
}
