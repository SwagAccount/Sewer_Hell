using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sandbox;

namespace trollface;

public sealed class DoorLocker : Component
{
	[Category("Lever")][Property] public Lever Lever {get;set;}
	[Property] public bool Flipped {get;set;}
	[Category("LockBox")][Property] public LockBox LockBox {get;set;}
	[Property] public Rotater rotater {get;set;}
	[Property] public Vector3 LockedAngles {get;set;}
	[Property] public Vector3 UnlockedAngles {get;set;}
	HingeJoint hingeJoint;
	protected override void OnStart()
	{
		hingeJoint = Components.Get<HingeJoint>();
	}

	protected override void OnUpdate()
	{
		bool On = false;

		if(Lever != null)
			On = Lever.On;

		if(LockBox!=null)
			On = LockBox.On;
		
		if(!Flipped)
			rotater.MaxAxis = On ? UnlockedAngles : LockedAngles;
		else
			rotater.MinAxis = On ? UnlockedAngles : LockedAngles;
		
		//if(LockBox != null)


	}
}
