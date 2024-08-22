using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sandbox;

namespace trollface;

public sealed class DoorLocker : Component
{
	[Property] public Lever Lever {get;set;}
	[Property] public Rotater rotater {get;set;}
	[Property] public bool Flipped {get;set;}
	[Property] public Vector3 LockedAngles {get;set;}
	[Property] public Vector3 UnlockedAngles {get;set;}
	HingeJoint hingeJoint;
	protected override void OnStart()
	{
		hingeJoint = Components.Get<HingeJoint>();
	}

	protected override void OnUpdate()
	{
		if(Lever == null) return;
		
		if(!Flipped)
			rotater.MaxAxis = Lever.On ? UnlockedAngles : LockedAngles;
		else
			rotater.MinAxis = Lever.On ? UnlockedAngles : LockedAngles;


	}
}
