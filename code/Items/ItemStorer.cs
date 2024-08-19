using System;
using System.Text.Json.Nodes;
using Sandbox;
namespace trollface;

public sealed class ItemStorer : Component
{
	[Property] public List<Item> items { get; set; } = new List<Item>();
	[Property] public List<string> Categories { get; set; } = new List<string>();
	[Property] public bool spawnItem { get; set; }
	[Property] public string StoredItem { get; set; }
	[Property] public float SearchRadius { get; set; } = 10;
	ChunkDealer chunkDealer;
	protected override void OnStart()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		if (spawnItem)
		{
			spawnItem = false;
			ReleaseItem();
		}

		var nearbyObjects = Scene.FindInPhysics(new Sphere(Transform.Position, SearchRadius));
		items = new List<Item>();
		foreach (var obj in nearbyObjects)
		{
			Item item = obj.Components.Get<Item>();
			if (!item.IsValid() || items.Contains(item)) continue;

			items.Add(item);
		}

		List<Item> itemsToRemove = new List<Item>();
		foreach (Item item in items)
		{
			if(!item.IsValid())
			{
				itemsToRemove.Add(item);
				continue;
			}
			if (item.TimeAlive > 1 && item.HandsConnected <= 0 && !item.Tags.Contains("container") && StoredItem == null && item.InCatagory(Categories))
			{
				StoreItem(item.GameObject);
				itemsToRemove.Add(item);
			}
		}

		while (itemsToRemove.Count > 0)
		{
			items.Remove(itemsToRemove[0]);
			itemsToRemove.RemoveAt(0);
		}
	}

	public void StoreItem(GameObject gameObject)
	{
		if (gameObject.IsPrefabInstance) gameObject.BreakFromPrefab();
		StoredItem = gameObject.Serialize().ToJsonString();
		gameObject.DestroyImmediate();
	}

	public Item ReleaseItem()
	{
		if (StoredItem == null) return null;
		GameObject newObject = new();
		newObject.Deserialize(Json.Deserialize<JsonObject>( StoredItem ));
		StoredItem = null;
		newObject.Transform.Position = Transform.Position;	
		return newObject.Components.Get<Item>();
	}
}
