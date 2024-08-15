using System;
using Sandbox;
namespace trollface;
public sealed class Lever : Component
{
	[Property] public bool On {get; set;}
	[Property] public HingeJoint Hinge {get; set;}
	[Property] public float AngleThreshold { get; set; } = 1.0f; // Threshold for detecting near min/max angle

    protected override void OnFixedUpdate()
    {
        if (Hinge == null) return;

        float currentAngle = Hinge.Angle;
        if (MathF.Abs(currentAngle - Hinge.MaxAngle) < AngleThreshold)
        {
            if (!On)
            {
                On = true;
				Hinge.TargetAngle = Hinge.MaxAngle;
                //playsound
            }
        }
        else if (MathF.Abs(currentAngle - Hinge.MinAngle) < AngleThreshold)
        {
            if (On)
            {
                On = false;
				Hinge.TargetAngle = Hinge.MinAngle;
                //playsound
            }
        }
    }
}
