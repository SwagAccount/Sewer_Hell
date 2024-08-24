using Sandbox;
namespace trollface;
public sealed class Holster : Component, Component.ITriggerListener
{
	[Property] public List<AllowedItem> AllowedItems{get;set;} = new List<AllowedItem>();
	[Property] public List<Item> items {get; set;} = new List<Item>();
	[Property] public GameObject ObjectRef {get;set;}

	public class AllowedItem
	{
		[KeyProperty] public string ItemName {get;set;}
		[KeyProperty] public Vector3 Position {get;set;}=  Vector3.Zero;
		[KeyProperty] public Rotation Rotation {get;set;} = Rotation.Identity;
	}
	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		if(GameObject.Children.Count > 0) return;
		Item item =  other.Components.GetInParentOrSelf<Item>();
		if(!item.IsValid()) return;
		if(items.Contains(item)) return;
		if(other.GameObject == ObjectRef) return;

		bool isAllowed = false;
		foreach(AllowedItem allowedItem in AllowedItems)
		{
			if(allowedItem.ItemName == item.ItemName)
			{
				isAllowed = true;
				break;
			}
		}
		if(!isAllowed) return;

		items.Add(item);
	}

	void ITriggerListener.OnTriggerExit(Collider other)
	{
		Item item = other.Components.Get<Item>();
		if(!item.IsValid()) return;
		if(!items.Contains(item)) return;
		items.Remove(item);
	}

	BoxCollider collider;

	protected override void OnStart()
	{
		collider = Components.Get<BoxCollider>();
	}
	AllowedItem currentAllowedItem;
	protected override void OnUpdate()
	{
		List<Item> itemsToRemove = new List<Item>();
		foreach(Item item in items)
		{
			if(!item.IsValid())
			{
				itemsToRemove.Add(item);
				continue;
			}
			if(item.HandsConnected <= 0)
			{
				if(GameObject.Children.Count == 0 || currentAllowedItem == null)
				{
					foreach(AllowedItem allowedItem in AllowedItems)
					{
						if(allowedItem.ItemName == item.ItemName)
						{
							currentAllowedItem = allowedItem;
						}
					}
				};
				item.GameObject.BreakFromPrefab();
				item.rigidbody.MotionEnabled = false;
				item.GameObject.SetParent(GameObject);
				item.Transform.LocalPosition = currentAllowedItem.Position; 
				item.Transform.LocalRotation = currentAllowedItem.Rotation; 
				foreach (var itm in items)
				{
					if(itm != item) itemsToRemove.Add(itm);
				}
			}
			else
			{
				item.rigidbody.MotionEnabled = true;
			}
		}
		while(itemsToRemove.Count > 0)
		{
			itemsToRemove.RemoveAt(0);
		}
	}
}
