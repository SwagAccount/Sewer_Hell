using Sandbox;
namespace trollface;
public sealed class RevolverCase : Component
{
	[Property] public float Length {get;set;}
	[Property] public Vector3 Direction {get;set;}
	[Property] public Rigidbody Rigidbody {get;set;}

	Vector3 startPos;
	protected override void OnStart()
	{
		startPos = Transform.LocalPosition;
	}
	protected override void OnUpdate()
	{
		if(!Rigidbody.MotionEnabled)
		{
			Transform.LocalPosition += Direction*Time.Delta;
			if(Vector3.DistanceBetween(Transform.LocalPosition,startPos) > Length)
			{
				Rigidbody.MotionEnabled = true;
				GameObject.SetParent(Scene);
			}
		}
		else
		{
			if(Transform.Position.z < -1000)
			{
				GameObject.Destroy();
			}
		}
		
	}
}
