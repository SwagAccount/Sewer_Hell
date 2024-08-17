using System.Globalization;
using Editor;
using Sandbox;
using trollface;

public sealed class GameManager : Component
{
	[Property] public float TimeM {get;set;}
	[Property] public float DayTime {get;set;} = 48f;
	[Property] public int FloodDays {get;set;}

	[Property] public float NextFlood {get;set;}

	[Property] public GameObject SpawnerParent {get;set;}

	List<Spawner> spawners {get;set;}
	ChunkDealer chunkDealer {get;set;}

	PlayerSaveManager playerSaveManager {get;set;}

	protected override void OnStart()
	{
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
		TimeM += Time.Delta/60;
		if(TimeM > NextFlood)
		{
			Flood();
			NextFlood = TimeM + (DayTime*FloodDays);
		}
	}

	public void Flood()
	{
		for (int x = 0; x < chunkDealer.chunks.Count; x++)
        {
            for (int y = 0; y < chunkDealer.chunks[x].Count; y++)
            {
				if(chunkDealer.SafeChunk == new Vector2(x,y)) continue;
				foreach(GameObject c in chunkDealer.chunks[x][y].GameObject.Children)
				{
					c.Destroy();
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
		if(!FileSystem.Data.DirectoryExists($"Saves/Slot1/{Scene.Name}")) FileSystem.Data.CreateDirectory($"Saves/Slot1/{Scene.Name}");

		chunkDealer.SaveAllChunks();
		playerSaveManager.Save();

		GameSaveData gameSaveData = new GameSaveData();
		gameSaveData.TimeM = TimeM;
		gameSaveData.NextFlood = NextFlood;
		FileSystem.Data.WriteAllText
		(
			$"Saves/Slot1/GameData.json",
			Json.Serialize(gameSaveData)
		);
	}

	class GameSaveData
	{
		public float TimeM {get;set;}
		public float NextFlood {get;set;}
	}

	public void Load()
	{
		if(!FileSystem.Data.DirectoryExists($"Saves/Slot1/{Scene.Name}")) return;

		string data = FileSystem.Data.ReadAllText($"Saves/Slot1/GameData.json");
		GameSaveData gameSaveData = Json.Deserialize<GameSaveData>(data);
		
		TimeM = gameSaveData.TimeM;
		NextFlood = gameSaveData.NextFlood;

		chunkDealer.LoadAllChunks();
		
		playerSaveManager.Load();
		Log.Info("balls");
	}
}
