using System.Diagnostics;
using Sandbox;
using trollface;

public sealed class GoToSceneSwitch : Component
{
	[Property] public Lever Switch {get;set;}
	[Property] public string scene {get;set;}= "scenes/menu.scene";

	bool Loading;
	Survival survival;
	protected override void OnStart()
	{
		survival = Scene.Components.GetInChildren<Survival>();
		base.OnStart();
	}
	protected override void OnUpdate()
	{
		if(Switch.On && !Loading)
		{
			Loading = true;
			survival.Transition(scene);
		}
	}
}
