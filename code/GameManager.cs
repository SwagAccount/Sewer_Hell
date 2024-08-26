using System.Globalization;
using Editor;
using Sandbox;
using trollface;

public sealed class GameManager : Component
{
	[Property] public string LevelName {get;set;} = "Menu";
	[Property] public float TimeMultplier {get;set;} = 1f;
	[Property] public float TimeM {get;set;}
	[Property] public float DayTime {get;set;} = 48f;
	[Property] public int FloodDays {get;set;}

	[Property] public float NextFlood {get;set;}
	[Property] public Curve FloodEffectCurve {get;set;}
	[Property] public Vector3 MaxWaterHeight {get;set;}
	[Property] public Vector2 DripEffectRate {get;set;}
	[Property] public Color WaterRed {get;set;}
	[Property] public ModelRenderer Water {get;set;}
	[Property] public ParticleEmitter DripEffect {get;set;}

	[Property] public GameObject SpawnerParent {get;set;}

	int SaveSlot = 0;

	List<Spawner> spawners {get;set;}
	ChunkDealer chunkDealer {get;set;}

	PlayerSaveManager playerSaveManager {get;set;}

	Vector3 waterStartPos;

	protected override void OnStart()
	{
		waterStartPos = Water.Transform.Position;
		if(FileSystem.Data.FileExists("SaveSlot.txt"))
			SaveSlot = int.Parse(FileSystem.Data.ReadAllText("SaveSlot.txt"));
		
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();
		playerSaveManager = Scene.Components.GetInChildren<PlayerSaveManager>();
		spawners = new List<Spawner>();
		foreach(GameObject c in SpawnerParent.Children)
		{
			Spawner spawner = c.Components.Get<Spawner>();
			if(spawner == null) continue;
			spawners.Add(spawner);
		}
		Load();
		
	}
	protected override void OnFixedUpdate()
	{
		float progress = FloodEffectCurve.Evaluate(1-((NextFlood-TimeM)/(DayTime*FloodDays)));

		DripEffect.Rate = MathX.Lerp(DripEffectRate.x,DripEffectRate.y, progress);
		Water.Transform.Position = Vector3.Lerp(waterStartPos, MaxWaterHeight, progress);
		Water.Tint = Color.Lerp(Color.White,WaterRed,progress);
		TimeM += (Time.Delta/60)*TimeMultplier;
		if(TimeM > NextFlood)
		{
			Flood();
			NextFlood = TimeM + (DayTime*FloodDays);
		}
	}

	public void Flood()
	{
		if(!chunkDealer.PlayerInSafeChunk())
		{
			playerSaveManager.healthComponent.Health = 0;
		}
		for (int x = 0; x < chunkDealer.chunks.Count; x++)
        {
            for (int y = 0; y < chunkDealer.chunks[x].Count; y++)
            {
				if(chunkDealer.SafeChunk == new Vector2(x,y)) continue;
				foreach(GameObject c in chunkDealer.chunks[x][y].GameObject.Children)
				{
					if(!c.Tags.Contains("dontdelete")) c.Destroy();
				}
            }
        }

		foreach(Spawner spawner in spawners)
		{
			spawner.Spawn();
		}

	}

	public void Save()
	{
		Log.Info("Save");
		if(!FileSystem.Data.DirectoryExists($"Saves/Slot{SaveSlot}/{LevelName}")) FileSystem.Data.CreateDirectory($"Saves/Slot{SaveSlot}/{LevelName}");

		chunkDealer.SaveAllChunks(SaveSlot, LevelName);
		playerSaveManager.Save(SaveSlot);

		GameSaveData gameSaveData = new GameSaveData();
		gameSaveData.TimeM = TimeM;
		gameSaveData.NextFlood = NextFlood;
		FileSystem.Data.WriteAllText
		(
			$"Saves/Slot{SaveSlot}/GameData.json",
			Json.Serialize(gameSaveData)
		);
	}

	public class GameSaveData
	{
		public float TimeM {get;set;}
		public float NextFlood {get;set;}
	}

	public void Load()
	{
		Log.Info("Load");
		if(!FileSystem.Data.DirectoryExists($"Saves/Slot{SaveSlot}/{LevelName}")) return;

		string data = FileSystem.Data.ReadAllText($"Saves/Slot{SaveSlot}/GameData.json");
		GameSaveData gameSaveData = Json.Deserialize<GameSaveData>(data);
		
		TimeM = gameSaveData.TimeM;
		NextFlood = gameSaveData.NextFlood;

		chunkDealer.LoadAllChunks(SaveSlot, LevelName);
		
		playerSaveManager.Load(SaveSlot);
		
	}
}
