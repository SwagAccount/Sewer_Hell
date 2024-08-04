using System;

namespace trollface;
public sealed class RevolverTrigger : Component
{
	[Property] public BarrelBase Barrel {get;set;}
	[Property] public RevolverCylinder RevolverCylinder {get;set;}
	[Property] public GameObject TriggerBone {get;set;}
    [Property] public Angles TargetTriggerRot {get;set;}
	[Property] public Angles TargetTriggerBackRot {get;set;}
	[Property] public GameObject HammerBone {get;set;}
	[Property] public Angles TargetHammerClosedRot {get;set;}
	[Property] public Angles TargetHammerOpenRot {get;set;}
	[Property] public float PullBackShoot {get;set;} = 0.75f;
	[Property] public float PullBackSpeed {get;set;} = 2f;
	[Property] public float TriggerSpeed {get;set;} = 10f;
    [Property] public SoundEvent CockSound {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject FingerLeft {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject FingerRight {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject TriggerSafeRef {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject TriggerRef {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject TriggerBackRef {get;set;}
    [Category("Finger Stuff")] [Property] public Vector3 LeftFingerPosMod {get;set;}
    [Category("Finger Stuff")] [Property] public Angles LeftFingerRotMod {get;set;}

    [Category("Finger Stuff Thumb")] [Property] public GameObject ThumbLeft {get;set;}
	[Category("Finger Stuff Thumb")] [Property] public GameObject ThumbRight {get;set;}
	[Category("Finger Stuff Thumb")] [Property] public GameObject HammerSafeRef {get;set;}
	[Category("Finger Stuff Thumb")] [Property] public GameObject HammerRef {get;set;}
	[Category("Finger Stuff Thumb")] [Property] public GameObject HammerBackRef {get;set;}

    
    public Item item;

	protected override void OnStart()
	{
		item = GameObject.Parent.Components.Get<Item>();
	}
    bool canFire = true;
    public float pullBack;

    bool pullingHammer;

    float triggerPullBackSmooth;

    float lastPullBack;
	protected override void OnUpdate()
	{

        if(!item.mainHeld) return;
        
        

        if(canFire && !RevolverCylinder.open)
        {
            triggerPullBackSmooth = MathX.Lerp(triggerPullBackSmooth, item.Controller.Trigger.Value, TriggerSpeed*Time.Delta);
            if(!triggerPullBackSmooth.AlmostEqual(0,0.01f))
            {
                
                pullBack = MathF.Max(pullBack, triggerPullBackSmooth);
                pullingHammer = false;
            }
            else if (item.Controller.ButtonA)
            {
                pullingHammer = true;
                pullBack = MathX.Clamp(pullBack + PullBackSpeed * Time.Delta, 0, 1);
                if(pullBack >= PullBackShoot && lastPullBack < PullBackShoot) Sound.Play(CockSound, HammerBone.Transform.Position);
            }
            HammerBone.Transform.LocalRotation = Angles.Lerp(TargetHammerClosedRot,TargetHammerOpenRot,pullBack);
            TriggerBone.Transform.LocalRotation = Angles.Lerp(TargetTriggerRot,TargetTriggerBackRot,pullBack);

            RevolverCylinder.RotateAmount = pullBack * (1/PullBackShoot);
            
            if(MathF.Min(pullBack, item.Controller.Trigger.Value) > PullBackShoot && !pullingHammer)
            {
                RevolverCylinder.LoadBarrel();
                Barrel.Fire();
                canFire = false;
            }

            lastPullBack = pullBack;
        }
        else
        {
            pullBack = 0;
            RevolverCylinder.RotateAmount = 0;
            triggerPullBackSmooth = 0;
            HammerBone.Transform.LocalRotation = TargetHammerClosedRot;
            TriggerBone.Transform.LocalRotation = TargetTriggerRot;
        }

        if(item.Controller.Trigger.Value.AlmostEqual(0,0.01f))
        {
            canFire = true;
        }

        SetFingerPos(
            item.Controller == Input.VR.RightHand ? FingerRight : FingerLeft,
            TriggerSafeRef, 
            TriggerRef, 
            TriggerBackRef, 
            item, 
            item.Controller == Input.VR.RightHand ? Vector3.One : LeftFingerPosMod, 
            item.Controller == Input.VR.RightHand ? new Angles(1,1,1) : LeftFingerRotMod,
            pullingHammer ? 0 : MathF.Min(pullBack, triggerPullBackSmooth),
            item.Controller.GetFingerValue(Sandbox.VR.FingerValue.IndexCurl) );

        SetFingerPos(
            item.Controller == Input.VR.RightHand ? ThumbRight : ThumbLeft,
            HammerSafeRef, 
            HammerRef, 
            HammerBackRef, 
            item, 
            item.Controller == Input.VR.RightHand ? Vector3.One : LeftFingerPosMod, 
            item.Controller == Input.VR.RightHand ? new Angles(1,1,1) : LeftFingerRotMod,
            pullingHammer ? pullBack : 0,
            item.Controller.ButtonA ? 1 : 0 );
	}

    public static void SetFingerPos(GameObject finger, GameObject triggerSafe, GameObject trigger, GameObject triggerBack, Item item, Vector3 posMod, Angles angMod, float pullBack, float fingerValue)
    {
        if(fingerValue < 0.5f) 
        {
            HandsDealer.CopyTransformRecursive(triggerSafe,finger, posMod, angMod);
        }
        else
        {
            HandsDealer.CopyTransformRecursiveLerp(trigger, triggerBack, finger, posMod, angMod, pullBack);
        }
    }
}
