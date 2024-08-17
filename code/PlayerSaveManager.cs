using System.Globalization;
using System.Text.Json.Nodes;
using Sandbox;
namespace trollface;

public sealed class PlayerSaveManager : Component
{
	HealthComponent healthComponent;
	Survival survival;
	Vrmovement vrmovement;
	ChunkDealer chunkDealer;
	protected override void OnAwake()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();

		healthComponent = Components.Get<HealthComponent>();
		survival = Components.Get<Survival>();
		vrmovement = Components.Get<Vrmovement>();
	}

	class PlayerSaveData
	{
		public Vector3 position {get;set;}
		public string healthComponent {get;set;}
		public string survival {get;set;}
		public List<string> HeldItems {get;set;}
	}

	public void Save()
	{
		PlayerSaveData playerSaveData = new PlayerSaveData
		{
			position = vrmovement.characterController.Transform.Position,
			healthComponent = healthComponent.Serialize().ToJsonString(),
			survival = survival.Serialize().ToJsonString(),
			HeldItems = new List<string>()
		};
		
		foreach(GameObject c in vrmovement.VRSpace.Children)
		{
			Item item = c.Components.Get<Item>();
			if(item == null) continue;
			if(c.IsPrefabInstance) c.BreakFromPrefab();
			playerSaveData.HeldItems.Add(c.Serialize().ToJsonString());
		}

		FileSystem.Data.WriteAllText
		(
			$"Saves/Slot1/PlayerData.json",
			Json.Serialize(playerSaveData)
		);
	}

	public void Load()
	{
		if(!FileSystem.Data.FileExists($"Saves/Slot1/PlayerData.json")) return;
		
		PlayerSaveData playerSaveData = Json.Deserialize<PlayerSaveData>(FileSystem.Data.ReadAllText($"Saves/Slot1/PlayerData.json"));
		healthComponent.Deserialize(Json.Deserialize<JsonNode>(playerSaveData.healthComponent).AsObject());
		
		survival.Deserialize(Json.Deserialize<JsonNode>(playerSaveData.survival).AsObject());
		
		foreach(string item in playerSaveData.HeldItems)
		{
			Log.Info(item);
			GameObject spawnedItem = new GameObject();
			spawnedItem.Deserialize(Json.Deserialize<JsonObject>(item));

			spawnedItem.Transform.Position = Transform.World.PointToWorld(spawnedItem.Transform.Position - playerSaveData.position);
			
			chunkDealer.PlaceInChunk(spawnedItem);
		}
		
	}
}
