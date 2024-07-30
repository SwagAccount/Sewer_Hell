using Sandbox;

public sealed class GrabPointFinder : Component, Component.ITriggerListener
{
	[Property] public List<GameObject> GrabbablePoints {get;set;}

	protected override void OnStart()
	{
		if(GrabbablePoints == null)
		{
			GrabbablePoints = new List<GameObject>();
		}
	}
	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		if(!other.Tags.Contains("grabpoint")) return;

		if(!GrabbablePoints.Contains(other.GameObject))
			GrabbablePoints.Add(other.GameObject);
	}

	void ITriggerListener.OnTriggerExit(Collider other)
	{
		if(!other.Tags.Contains("grabpoint")) return;

		if(GrabbablePoints.Contains(other.GameObject))
			GrabbablePoints.Remove(other.GameObject);
	}
}
