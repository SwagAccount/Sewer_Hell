using System;
using Sandbox;
using trollface;

public sealed class MainMenuDealer : Component
{
	[Property] public int SaveSlot {get;set;}
	[Property] public float DayTime {get;set;} = 30;
	[Property] public int FloodDays {get;set;} = 1;
	[Property] public Slider SaveSlotSlider {get;set;}
	[Property] public NixieTubeNumber SaveSlotDisplay {get;set;}
	[Property] public NixieRowNumber PlayTime {get;set;}
	[Property] public NixieRowNumber FloodN  {get;set;}
	[Property] public NixieRowNumber NextFlood {get;set;}
	[Property] public Lever PlayLever {get;set;}

	Survival survival;
	protected override void OnStart()
	{
		survival = Scene.Components.GetInChildren<Survival>();
		if(FileSystem.Data.FileExists("SaveSlot.txt"))
			SaveSlot = int.Parse(FileSystem.Data.ReadAllText("SaveSlot.txt"));

		SaveSlotSlider.SetValue(SaveSlot);
		SaveSlotDisplay.SetNumber(SaveSlot);
	}
	protected override void OnUpdate()
	{
		if(!Loaded && PlayLever.On )
			LoadIntoGame();
		if(SaveSlotSlider.incrementUpdated)
		{
			updateData();
		}
	}
	bool Loaded = false;
	void LoadIntoGame()
	{
		Loaded = true;
		FileSystem.Data.WriteAllText("SaveSlot.txt", SaveSlot.ToString());
		survival.Transition("scenes/level1.scene");
	}

	async void updateData()
	{
		int value = SaveSlotSlider.GetValue();
		SaveSlot = value;
		await SaveSlotDisplay.SetNumber(SaveSlotSlider.GetValue());
		GameManager.GameSaveData gameSaveData = null;
		if(FileSystem.Data.DirectoryExists($"Saves/Slot{SaveSlot}"))
			gameSaveData = Json.Deserialize<GameManager.GameSaveData>(FileSystem.Data.ReadAllText($"Saves/Slot{SaveSlot}/GameData.json"));
		Log.Info(gameSaveData);
		PlayTime.SetNumber(gameSaveData == null ? -1 : (int)MathF.Round(gameSaveData.TimeM));
		FloodN.SetNumber(gameSaveData == null ? -1 : (int)MathF.Round(gameSaveData.TimeM/(DayTime*FloodDays) + 1));
		NextFlood.SetNumber(gameSaveData == null ? -1 : (int)MathF.Round(gameSaveData.NextFlood - gameSaveData.TimeM));
	}
}
