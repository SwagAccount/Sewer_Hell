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
	[Property] public SoundEvent ClickSound {get;set;} = ResourceLibrary.Get<SoundEvent>("sounds/switch.sound");

    protected override void OnFixedUpdate()
    {
        float currentAngle;
        currentAngle = Rotater.Rotated.Transform.LocalRotation.Angles().AsVector3().Length; 
        if (MathF.Abs(currentAngle - MaxAxis.Length) < AngleThreshold)
        {
            if(On == Flipped)
                Sound.Play(ClickSound,Transform.Position);
            On = !Flipped;
        }
        else if (MathF.Abs(currentAngle - Rotater.MinAxis.Length) < AngleThreshold)
        {
            if(On == !Flipped)
                Sound.Play(ClickSound,Transform.Position);
            On = Flipped;
        }
    }
}
