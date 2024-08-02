using System;
using Microsoft.CSharp.RuntimeBinder;
using Sandbox;
namespace trollface;


public sealed class Vrmovement : Component
{
	[Property] public ColorAdjustments Camera {get;set;}
	[Property] public CharacterController characterController {get;set;}
	[Property] public GameObject VRSpace {get;set;}
	[Property] public float WalkSpeed {get;set;} = 190f;
	[Property] public float CrouchSpeed {get;set;} = 64f;
	[Property] public float OverDistance {get;set;} = 12f;
	[Property] public float HeadRadius {get;set;} = 4f;
	[Property] public float DarknessSpeed {get;set;} = 20f;
	[Property] public float CrouchDistance {get;set;} = 19;
	[Property] public float PhysicalCrouchHeight {get;set;} = 40;
	public Vector3 offset {get;set;}

    float CamHeight;
    bool hideCam;
    bool crouching;

    bool crouchSpeed;

    Vector3 wantedVRSpacePos;
	protected override void OnUpdate()
	{
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

        Camera.Brightness = MathX.Lerp(Camera.Brightness, hideCam ? 0 : 1, Time.Delta * DarknessSpeed);
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
        Vector3 setPosition = Camera.Transform.Position.WithZ(characterController.Transform.Position.z);
        characterController.MoveTo(Camera.Transform.Position.WithZ(characterController.Transform.Position.z),true);
        if(characterController.Transform.Position != setPosition && Vector3.DistanceBetween(characterController.Transform.Position.WithZ(0),Camera.Transform.Position.WithZ(0)) > OverDistance)
        {
            Log.Info("sex");
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

        characterController.Velocity = wishVelocity;

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
}
