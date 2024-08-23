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
	[Property] public Lever TutorialLever {get;set;}
	[Property] public Lever ResetLever {get;set;}

	Survival survival;

	bool hasReset;
	protected override void OnStart()
	{
		survival = Scene.Components.GetInChildren<Survival>();
		if(FileSystem.Data.FileExists("SaveSlot.txt"))
			SaveSlot = int.Parse(FileSystem.Data.ReadAllText("SaveSlot.txt"));

		SaveSlotSlider.SetValue(SaveSlot);
		SaveSlotDisplay.SetNumber(SaveSlot);

		updateData();
	}

	protected override void OnUpdate()
	{
		if(!hasReset && ResetLever.On)
		{
			hasReset = true;
			ResetSlot();
			ResetLever.Rotater.MaxAxis = ResetLever.Rotater.MinAxis;
		}
		else if (!ResetLever.On)
		{
			hasReset = false;
			ResetLever.Rotater.MaxAxis = ResetLever.MaxAxis;
		}

		if(!Loaded && PlayLever.On )
			LoadIntoGame("scenes/level1.scene");

		if(!Loaded && TutorialLever.On)
			LoadIntoGame("scenes/tutorial.scene");
		
		if(SaveSlotSlider.incrementUpdated)
		{
			updateData();
		}
	}
	bool Loaded = false;
	void ResetSlot()
	{
		if(!FileSystem.Data.DirectoryExists($"Saves/Slot{SaveSlot}")) return;
		FileSystem.Data.DeleteDirectory($"Saves/Slot{SaveSlot}" , true);
		updateData();
	}

	void LoadIntoGame(string scene)
	{
		Loaded = true;
		FileSystem.Data.WriteAllText("SaveSlot.txt", SaveSlot.ToString());
		survival.Transition(scene);
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
