using Sandbox;
namespace trollface;

public sealed class Container : Component, Component.ITriggerListener
{
	[Property] public List<Item> items {get; set;} = new List<Item>();
	[Property] public GameObject ObjectRef {get;set;}
	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		Item item =  other.Components.GetInParentOrSelf<Item>();
		if(!item.IsValid()) return;
		if(items.Contains(item)) return;
		if(other.GameObject == ObjectRef) return;
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
				item.GameObject.BreakFromPrefab();
				item.rigidbody.MotionEnabled = false;
				item.GameObject.SetParent(GameObject);
			}
			else
			{
				item.rigidbody.MotionEnabled = true;
				DrawBoxCorners(Transform.Position, Transform.Rotation, collider.Scale, Color.Green);
			}
		}
		while(itemsToRemove.Count > 0)
		{
			itemsToRemove.RemoveAt(0);
		}
	}

	public void DrawBoxCorners(Vector3 pos, Rotation rotation, Vector3 size, Color color)
	{
		Gizmo.Draw.Color = color;
		Gizmo.Draw.LineThickness = 10;
		Transform transform = new Transform
		{
			Position = pos,
			Rotation = rotation
		};
		
		for(int x = 0; x < 2; x++)
		{
			for(int y = 0; y < 2; y++)
			{
				for(int z = 0; z < 2; z++)
				{
					Vector3 corner = new Vector3(x-0.5f,y-0.5f,z-0.5f);

					Vector3 xLine = corner + (x == 1 ? new Vector3(-0.3f,0,0): new Vector3(0.3f,0,0));
					Vector3 yLine = corner + (y == 1 ? new Vector3(0,-0.3f,0): new Vector3(0,0.3f,0));
					Vector3 zLine = corner + (z == 1 ? new Vector3(0,0,-0.3f): new Vector3(0,0,0.3f));

					Gizmo.Draw.Line(transform.PointToWorld(corner*size),transform.PointToWorld(xLine*size));	
					Gizmo.Draw.Line(transform.PointToWorld(corner*size),transform.PointToWorld(yLine*size));	
					Gizmo.Draw.Line(transform.PointToWorld(corner*size),transform.PointToWorld(zLine*size));			
				}
			}
		}
	}
}
