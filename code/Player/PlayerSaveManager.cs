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
	[Property] public List<ItemStorer> ItemStores {get;set;}
	protected override void OnAwake()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();
		healthComponent = Components.Get<HealthComponent>();
		survival = Components.Get<Survival>();
		vrmovement = Components.Get<Vrmovement>();
		GameObject.BreakFromPrefab();
	}

	class PlayerSaveData
	{
		public Vector3 position {get;set;}
		public string healthComponent {get;set;}
		public string survival {get;set;}
		public List<string> HeldItems {get;set;}
		public List<string> ItemStores {get;set;}
	}

	public void Save(int slot)
	{
		PlayerSaveData playerSaveData = new PlayerSaveData
		{
			position = vrmovement.characterController.Transform.Position,
			healthComponent = healthComponent.Serialize().ToJsonString(),
			survival = survival.Serialize().ToJsonString(),
			HeldItems = new List<string>(),
			ItemStores = new List<string>()
		};
		
		foreach(GameObject c in vrmovement.VRSpace.Children)
		{
			Item item = c.Components.Get<Item>();
			if(item == null) continue;
			if(c.IsPrefabInstance) c.BreakFromPrefab();
			JsonObject serialized = c.Serialize();
			SceneUtility.MakeIdGuidsUnique(serialized);
			playerSaveData.HeldItems.Add(serialized.ToJsonString());
		}

		foreach(ItemStorer itemStorer in ItemStores)
		{
			playerSaveData.ItemStores.Add(itemStorer.StoredItem);
		}

		FileSystem.Data.WriteAllText
		(
			$"Saves/Slot{slot}/PlayerData.json",
			Json.Serialize(playerSaveData)
		);
	}

	public void Load(int slot)
	{
		if(!FileSystem.Data.FileExists($"Saves/Slot{slot}/PlayerData.json")) return;
		
		PlayerSaveData playerSaveData = Json.Deserialize<PlayerSaveData>(FileSystem.Data.ReadAllText($"Saves/Slot{slot}/PlayerData.json"));
		healthComponent.Deserialize(Json.Deserialize<JsonNode>(playerSaveData.healthComponent).AsObject());
		
		survival.Deserialize(Json.Deserialize<JsonNode>(playerSaveData.survival).AsObject());
		
		foreach(string item in playerSaveData.HeldItems)
		{
			GameObject spawnedItem = new GameObject();
			spawnedItem.Deserialize(Json.Deserialize<JsonObject>(item));

			Item itemC = spawnedItem.Components.Get<Item>();

			itemC.mainHeld = false;	

			foreach(HandPos handPos in itemC.HandPoss)
			{
				if(handPos.GameObject.Parent.Tags.Contains("grabbed")) handPos.GameObject.Parent.Tags.Remove("grabbed");
			}

			Rigidbody rigidbody = spawnedItem.Components.Get<Rigidbody>();
			rigidbody.MotionEnabled = false;

			//spawnedItem.Transform.Position = Transform.World.PointToWorld(playerSaveData.position - spawnedItem.Transform.Position);
			
			chunkDealer.PlaceInChunk(spawnedItem);
		}

		for(int i = 0; i < ItemStores.Count; i++)
		{
			Log.Info(playerSaveData.ItemStores[i]);
			ItemStores[i].StoredItem = playerSaveData.ItemStores[i];
		}
		
	}
}
