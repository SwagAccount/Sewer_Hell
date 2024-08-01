using System;
using Sandbox;
namespace trollface;


public sealed class Vrmovement : Component
{
	[Property] private GameObject camera {get;set;}
    [Property] private GameObject cameraOffset{get;set;}
    [Property] private GameObject actCameraOffset{get;set;}
    [Property] private float defaultHeight {get;set;}
    [Property] private float headheight {get;set;}
    [Property] private CapsuleCollider capsuleCollider {get;set;}

    [Property] public float _speed {get;set;}
    [Property] public float updateRBDis {get;set;}
    [Property] public float _speedSmoothing {get;set;}
    private Rigidbody _rig;
    private Vector2 _input;
    private Vector3 _movementVector;
    private Vector3 _moveDirection;
    Vector3 lastPos;
    protected override void OnStart()
    {
        lastPos = Transform.Position;
        _rig = Components.Get<Rigidbody>();
		PhysicsLock physicsLock = new PhysicsLock
		{
			Pitch = true,
			Roll = true,
			Yaw = true
		};
		_rig.Locking = physicsLock;
	}
    float speed;

    float posX;
    float posY;
    float heightMax;
    protected override void OnUpdate()
    {
        
        _input = Vector2.Zero;//Input.VR.LeftHand.Joystick;
        if (_input.Length > 0 || Vector3.DistanceBetween(Transform.World.PointToWorld(capsuleCollider.Start).WithZ(0),camera.Transform.Position.WithZ(0)) >= updateRBDis)
        {
            posX = cameraOffset.Transform.LocalPosition.x;
            posY = cameraOffset.Transform.LocalPosition.y;
        }

        heightMax = defaultHeight;
		Vector3 hitPos = new Vector3(cameraOffset.Transform.Position.x, cameraOffset.Transform.Position.y, cameraOffset.Transform.Position.z);
		var hit = Scene.Trace.Ray(hitPos, hitPos + Transform.World.Up*150).WithTag("world").Run();
        if (hit.Hit)
        {
            var hit2 = Scene.Trace.Ray(hit.HitPosition, hit.HitPosition-Vector3.Up*150).WithTag("world").Run();
            if (hit2.Hit)
            {
                //Log.Info(hit2.GameObject.Name);
                heightMax = hit2.Distance-headheight;
            }
        }
        actCameraOffset.Transform.LocalPosition = new Vector3(0, 0, MathX.Clamp(Transform.Position.z + heightMax - cameraOffset.Transform.LocalPosition.z,-10000,0));

        capsuleCollider.Start = new Vector3(posX,posY,0)+(Vector3.Up*capsuleCollider.Radius);
		capsuleCollider.End = capsuleCollider.Start + (Vector3.Up*(camera.Transform.Position.z - Transform.Position.z)) - (Vector3.Up*capsuleCollider.Radius);
        
        Vector3 forwardDirection = camera.Transform.World.Forward;
        forwardDirection.z = 0; 
        forwardDirection = forwardDirection.Normal;
        Vector3 RightDirection = camera.Transform.World.Right;
        RightDirection.z = 0;
        RightDirection = RightDirection.Normal;

        _movementVector = Vector3.Lerp(_movementVector, _input.x * RightDirection * _speed + _input.y * forwardDirection * _speed, _speedSmoothing * Time.Delta);
        _rig.Velocity = new Vector3(_movementVector.x, _movementVector.y, _rig.Velocity.z);
    }
    private bool IsGrounded()
    {
		var hit = Scene.Trace.Ray(capsuleCollider.Start, capsuleCollider.Start+Vector3.Down * (capsuleCollider.Radius+5) ).WithTag("solid").Run();
        return hit.Hit;
    }
}
