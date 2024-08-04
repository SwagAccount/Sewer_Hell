using System;
using Microsoft.CSharp.RuntimeBinder;
using Sandbox;
namespace trollface;


public sealed class Vrmovement : Component
{
	[Property] public ColorAdjustments Camera {get;set;}
	[Property] public ManualHitbox Hitbox {get;set;}
	[Property] public CharacterController characterController {get;set;}
	[Property] public GameObject VRSpace {get;set;}
	[Property] public float RotateSpeed {get;set;} = 190f;
	[Property] public float WalkSpeed {get;set;} = 64f;
	[Property] public float CrouchSpeed {get;set;} = 22f;
	[Property] public float OverDistance {get;set;} = 16f;
	[Property] public float HeadRadius {get;set;} = 4f;
	[Property] public float DarknessSpeed {get;set;} = 20f;
	[Property] public float CrouchDistance {get;set;} = 19f;
	[Property] public float PhysicalCrouchHeight {get;set;} = 40f;
	[Property] public float StunTime {get;set;} = 5f;
	public Vector3 offset {get;set;}
    public float Stunned {get;set;}

    float CamHeight;
    bool hideCam;
    bool crouching;

    bool crouchSpeed;

    Vector3 wantedVRSpacePos;
    float reverseStun;
	protected override void OnUpdate()
	{
        Stunned = MathX.Lerp(Stunned,0,(1/StunTime)*Time.Delta);
        reverseStun = MathX.Clamp(1-Stunned,0,1);

        SetCrouch();
        crouchSpeed = crouching ? true : Camera.Transform.Position.z  - characterController.Transform.Position.z <= PhysicalCrouchHeight;

        hideCam = false;
        var heightCheck = HeightCheck();
        if(heightCheck.Hit)
        {
            CamHeight = heightCheck.Distance-HeadRadius;
            hideCam = true;
        }
        characterController.Height = CamHeight;

        if(Input.VR.LeftHand.Joystick.Value.Length > 0) Movement();
        else StationaryMovement();
        

        VRSpace.Transform.Position = wantedVRSpacePos;

        FitHitbox(Hitbox, Camera.Transform.Position.WithZ(characterController.Transform.Position.z), Camera.Transform.Position, GameObject);

        Camera.Brightness = MathX.Lerp(Camera.Brightness, hideCam ? 0 : 1, Time.Delta * DarknessSpeed);

        RotateAroundPoint(VRSpace, Camera.Transform.Position, Vector3.Up,Input.VR.RightHand.Joystick.Value.x*Time.Delta*-RotateSpeed);
        wantedVRSpacePos = VRSpace.Transform.Position;
	}

    SceneTraceResult HeightCheck(float addedHeight = 0)
    {
        CamHeight = Camera.Transform.Position.z  - characterController.Transform.Position.z + HeadRadius + addedHeight;
        return Scene.Trace.Ray(characterController.Transform.Position,characterController.Transform.Position+Vector3.Up*CamHeight).WithTag("world").Run();
    }
    void SetCrouch()
    {
        if(!crouching)
        {
            if(Input.VR.RightHand.JoystickPress.IsPressed && !Input.VR.RightHand.JoystickPress.WasPressed)
            {
                crouching = true;
                wantedVRSpacePos += Vector3.Down*CrouchDistance;
            }
        }
        else
        {
            if(Input.VR.RightHand.JoystickPress.IsPressed && !Input.VR.RightHand.JoystickPress.WasPressed && !HeightCheck(CrouchDistance).Hit)
            {
                crouching = false;
                wantedVRSpacePos += Vector3.Up*CrouchDistance;
            }
        }
    }
    void StationaryMovement()
    {
        characterController.Velocity = 0;
        Vector3 setPosition = Camera.Transform.Position.WithZ(characterController.Transform.Position.z);
        characterController.MoveTo(Camera.Transform.Position.WithZ(characterController.Transform.Position.z),true);
        if(characterController.Transform.Position != setPosition && Vector3.DistanceBetween(characterController.Transform.Position.WithZ(0),Camera.Transform.Position.WithZ(0)) > OverDistance*reverseStun)
        {
            var dis = Vector3.DistanceBetween(characterController.Transform.Position,setPosition); 
            wantedVRSpacePos = wantedVRSpacePos + (characterController.Transform.Position-setPosition).Normal * (dis - characterController.Radius);
        }
        var dir = Camera.Transform.Position.WithZ(0) - characterController.Transform.Position.WithZ(0);

        var inWallCheck = Scene.Trace.Ray(characterController.Transform.Position.WithZ(Camera.Transform.Position.z), Camera.Transform.Position+dir*HeadRadius ).WithTag("world").Run();
        if(inWallCheck.Hit) hideCam = true;
    }

    void Movement()
    {
        Vector3 wishVelocity = (
            Input.VR.LeftHand.Joystick.Value.y * Camera.Transform.World.Forward) + 
            (Input.VR.LeftHand.Joystick.Value.x * Camera.Transform.World.Right);

        if(crouchSpeed) wishVelocity *= CrouchSpeed;
        else wishVelocity *= WalkSpeed;

        characterController.Velocity = wishVelocity * reverseStun;

        wantedVRSpacePos += characterController.Transform.Position.WithZ(0)-Camera.Transform.Position.WithZ(0);

        characterController.Move();
    }
    public static bool IsPointInsideBoundingBox(Vector3 point, BBox bBox, GameObject relativeObject = null)
    {
        Vector3 mins = relativeObject == null ? bBox.Mins : relativeObject.Transform.World.PointToWorld(bBox.Mins);
        Vector3 maxs = relativeObject == null ? bBox.Maxs : relativeObject.Transform.World.PointToWorld(bBox.Maxs);

        return 
        point.x >= mins.x && point.x <= maxs.x &&
        point.y >= mins.y && point.y <= maxs.y &&
        point.z >= mins.z && point.z <= maxs.z;
    }

    public static void FitHitbox(ManualHitbox hB, Vector3 start, Vector3 end, GameObject relativeObject = null)
    {
        if(relativeObject != null)
        {
            start = relativeObject.Transform.World.PointToLocal(start);
            end = relativeObject.Transform.World.PointToLocal(end);
        }

        var dir = (end - start).Normal;

        hB.CenterA = start + dir*hB.Radius;
        hB.CenterB = end - dir*hB.Radius;
    }
    static void RotateAroundPoint(GameObject objectToRotate, Vector3 point, Vector3 axis, float angle)
    {
        Vector3 dir = objectToRotate.Transform.Position - point;

        Rotation rotation = Rotation.FromAxis(axis, angle);

        dir = rotation * dir;

        objectToRotate.Transform.Position = point + dir;

        objectToRotate.Transform.Rotation = rotation * objectToRotate.Transform.Rotation;
    }
}
