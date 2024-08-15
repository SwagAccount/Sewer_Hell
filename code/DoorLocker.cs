using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sandbox;

namespace trollface;

public sealed class DoorLocker : Component
{
	[Property] public Lever Lever {get;set;}
	[Property] public Vector2 LockedAngles {get;set;}
	[Property] public Vector2 UnlockedAngles {get;set;}
	HingeJoint hingeJoint;
	protected override void OnStart()
	{
		hingeJoint = Components.Get<HingeJoint>();
	}
	protected override void OnUpdate()
	{
		hingeJoint.MinAngle = Lever.On ? UnlockedAngles.x : LockedAngles.x;
		hingeJoint.MaxAngle = Lever.On ? UnlockedAngles.y : LockedAngles.y;
	}
}
