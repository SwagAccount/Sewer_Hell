using Sandbox;

public sealed class HandTargetGizmos : Component
{
	[Property] private bool AutoCalculateHoldPos {get;set;}
	[Property] private float sphereCheckRadius {get;set;} = 0.25f;
	[Property] private int checkIterations { get; set; } = 15;
	[Property] private GameObject testRotate {get;set;}
	[Property] private List<GameObject> fingers {get;set;}

	bool lastAutoCalc;
	protected override void DrawGizmos()
	{
		DrawSkeletonRecursive(GameObject);

		if(AutoCalculateHoldPos && !lastAutoCalc)
		{
			CalculateFingerPositions();
			Log.Info("sex");
		}

		lastAutoCalc = AutoCalculateHoldPos;
	}
	void DrawSkeletonRecursive(GameObject parent)
    {
        foreach (GameObject child in parent.Children)
        {
            Vector3 parentPosition = Transform.World.PointToLocal(parent.Transform.Position);
            Vector3 childPosition = Transform.World.PointToLocal(child.Transform.Position);
            
            Gizmo.Draw.Arrow(parentPosition, childPosition,0.5f, 0.1f);

            DrawSkeletonRecursive(child);
        }
    }

	private void CalculateFingerPositions()
	{
		foreach (var finger in fingers)
		{
			RotateFingerUntilTouch(finger);
		}
	}

	private void RotateFingerUntilTouch(GameObject finger)
	{
		var fingerParts = GetFingerParts(finger);
		var endChild = GetEndChild(finger);
		for (int i = 0; i < checkIterations; i++)
		{
			foreach (var part in fingerParts)
			{
				part.Transform.Rotation -= Rotation.FromToRotation(part.Transform.World.Right, part.Transform.World.Forward) / checkIterations;
				
				if (Scene.FindInPhysics(new Sphere(endChild.Transform.Position, sphereCheckRadius)).Count() > 0)
				{
					return;
				}
			}
		}
	}

	private List<GameObject> GetFingerParts(GameObject finger)
	{
		List<GameObject> parts = new List<GameObject>();
		CollectChildObjects(finger, parts);
		return parts;
	}

	private void CollectChildObjects(GameObject obj, List<GameObject> collection)
	{
		foreach (var child in obj.Children)
		{
			collection.Add(child);
			CollectChildObjects(child, collection);
		}
	}

	private GameObject GetEndChild(GameObject obj)
	{
		if (obj.Children.Count == 0)
		{
			return obj;
		}
		return GetEndChild(obj.Children[obj.Children.Count - 1]);
	}
}
