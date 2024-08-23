using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.VR;
using trollface;

public sealed class GrabPointFinder : Component, Component.ITriggerListener
{
	[Property] public List<GameObject> GrabbablePoints {get;set;} = new List<GameObject>();
	[Property] public GameObject FarGrabPoint {get;set;}
	[Property] public List<Interactable> InteractablePoints {get;set;} = new List<Interactable>();
	[Property] public List<ItemStorer> ItemStorers {get;set;} = new List<ItemStorer>();
	[Property] public string handName {get;set;} = "right";
	[Property] public float searchRadiusHand {get;set;} = 10;
	[Property] public float searchDistance {get;set;} = 100;
	[Property] public float searchDistanceRadius {get;set;} = 4;

	[Property] public bool RightController {get;set;}

	public Vector3 searchPoint;
	VRController controller;
	protected override void OnStart()
	{
		controller = RightController ? Input.VR.RightHand : Input.VR.LeftHand;
	}

	protected override void OnFixedUpdate()
	{
		if(GameObject.Children.Count == 0) return;

		for(int i = 0; i < 2; i++)
		{
			Vector3 searchPos = Transform.Position;
			if(i > 0)
			{
				var ray = Scene.Trace.Ray(FarGrabPoint.Transform.Position, FarGrabPoint.Transform.Position+FarGrabPoint.Transform.World.Forward*searchDistance).Radius(searchDistanceRadius).Run();
				if(ray.Hit) searchPos = ray.HitPosition;
			}
			IEnumerable<GameObject> gameObjects = Scene.FindInPhysics(new Sphere(searchPos,searchRadiusHand));
			GrabbablePoints = new List<GameObject>();
			InteractablePoints = new List<Interactable>();
			ItemStorers = new List<ItemStorer>();
			
			foreach(GameObject g in gameObjects)
			{
				if(i > 0 && g.Tags.Contains("closepickup")) continue;
				
				if(g.Tags.Contains("interactable"))
				{
					i = 10;
					InteractablePoints.Add(g.Components.Get<Interactable>());
				}
				if(g.Tags.Contains("itemstore") && i == 0)
				{
					i = 10;
					ItemStorers.Add(g.Components.Get<ItemStorer>());
				}
				if(g.Tags.Contains("grabpoint") && g.Tags.Contains(handName))
				{
					i = 10;
					HandPos handPos = g.Components.Get<HandPos>();
					if(handPos.Main || handPos.ShowWithoutMain || handPos.item.mainHeld) GrabbablePoints.Add(g);;
				}
			}

			searchPoint = GameObject.Children[0].Transform.Position;
		}
		
	}

	/*
	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		if(!other.Tags.Contains("grabpoint") || !other.Tags.Contains(handName)) return;

		if(!GrabbablePoints.Contains(other.GameObject))
			GrabbablePoints.Add(other.GameObject);
	}

	void ITriggerListener.OnTriggerExit(Collider other)
	{
		if(!other.Tags.Contains("grabpoint")) return;

		if(GrabbablePoints.Contains(other.GameObject))
			GrabbablePoints.Remove(other.GameObject);
	}
	*/
}
