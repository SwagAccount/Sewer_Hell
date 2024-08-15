using System.Text.Json.Nodes;
using Sandbox;
namespace trollface;

public sealed class ItemStorer : Component, Component.ITriggerListener
{
	[Property] public List<Item> items {get; set;} = new List<Item>();
	[Property] public bool spawnItem {get;set;}
	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		Item item =  other.Components.Get<Item>();
		if(!item.IsValid()) return;
		if(items.Contains(item)) return;
		items.Add(item);
	}

	void ITriggerListener.OnTriggerExit(Collider other)
	{
		Item item = other.Components.Get<Item>();
		if(!item.IsValid()) return;
		if(items.Contains(item))
			items.Remove(item);
	}
	[Property] public JsonObject StoredItem {get;set;}
	public void StoreItem(GameObject gameObject)
	{
		if(gameObject.IsPrefabInstance) gameObject.BreakFromPrefab();
		StoredItem = gameObject.Serialize();
		gameObject.DestroyImmediate();
	}

	public Item ReleaseItem()
	{
		
		if(StoredItem == null) return null;
		GameObject newObject = new();
		newObject.Deserialize( StoredItem );
		StoredItem = null;
		return newObject.Components.Get<Item>();
	}
	protected override void OnUpdate()
	{
		//Log.Info(StoredItem.ToString());
		if(spawnItem)
		{
			spawnItem = false;
			ReleaseItem();
		}
		List<Item> itemsToRemove = new List<Item>();
		foreach(Item item in items)
		{
			if(item.TimeAlive > 5 && StoredItem == null)
			{
				StoreItem(item.GameObject);
				itemsToRemove.Add(item);
			}
		}
		while(itemsToRemove.Count > 0)
		{
			items.Remove(itemsToRemove[0]);
			itemsToRemove.RemoveAt(0);
		}
	}
}
