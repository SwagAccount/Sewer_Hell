using System;
using Sandbox;
namespace trollface;
public sealed class Lever : Component
{
	[Property] public bool On {get; set;}
	[Property] public bool Flipped {get; set;}
	[Property] public Rotater Rotater {get; set;}
	[Property] public Vector3 MaxAxis {get; set;}
	[Property] public float AngleThreshold { get; set; } = 1.0f;

    protected override void OnFixedUpdate()
    {
        float currentAngle;
        currentAngle = Rotater.Rotated.Transform.LocalRotation.Angles().AsVector3().Length; 
        Log.Info(MathF.Abs(MaxAxis.Length));
        if (MathF.Abs(currentAngle - MaxAxis.Length) < AngleThreshold)
        {
            Log.Info("balls");
            On = !Flipped;
        }
        else if (MathF.Abs(currentAngle - Rotater.MinAxis.Length) < AngleThreshold)
        {
            On = Flipped;
        }
    }
}
