using Sandbox;
using Sandbox.Diagnostics;

public sealed class GrabPointFinder : Component, Component.ITriggerListener
{
	[Property] public List<GameObject> GrabbablePoints {get;set;}
	[Property] public string handName {get;set;} = "right";
	[Property] public float searchRadiusHand {get;set;} = 4;

	public Vector3 searchPoint;

	protected override void OnStart()
	{
		if(GrabbablePoints == null)
		{
			GrabbablePoints = new List<GameObject>();
		}
	}

	protected override void OnFixedUpdate()
	{
		if(GameObject.Children.Count == 0) return;
		IEnumerable<GameObject> gameObjects = Scene.FindInPhysics(new Sphere(GameObject.Children[0].Transform.Position,searchRadiusHand));
		GrabbablePoints = new List<GameObject>();
		
		foreach(GameObject g in gameObjects)
		{
			if(!g.Tags.Contains("grabpoint") || !g.Tags.Contains(handName)) continue;

			GrabbablePoints.Add(g);
		}

		searchPoint = GameObject.Children[0].Transform.Position;
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
