using System;

namespace trollface;
public sealed class MultiBarrelTrigger : Component
{
	[Property] public BarrelBase[] Barrels {get;set;}

	[Property] public GameObject[] TriggerBones {get;set;}
    [Property] public Angles[] TargetTriggerRots {get;set;}
	[Property] public Angles[] TargetTriggerBackRots {get;set;}
	[Property] public float PullBackShoot {get;set;} = 0.75f;

	[Category("Finger Stuff Index")] [Property] public GameObject FingerLeft {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject FingerRight {get;set;}

	[Category("Finger Stuff Index")] [Property] public GameObject TriggerSafeRef {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject[] TriggerRefs {get;set;}
	[Category("Finger Stuff Index")] [Property] public GameObject[] TriggerBackRefs {get;set;}
    [Category("Finger Stuff")] [Property] public Vector3 LeftFingerPosMod {get;set;}
    [Category("Finger Stuff")] [Property] public Angles LeftFingerRotMod {get;set;}

    
    public Item item;
    public MultiBarrelMagazine multiBarrelMagazine;
    protected override void OnEnabled()
	{
        if(!item.mainHeld) return;
		canShoot = item.Controller.Trigger.Value < 0.75f;
	}
	protected override void OnAwake()
	{
        multiBarrelMagazine = Components.Get<MultiBarrelMagazine>();
		item = GameObject.Parent.Components.Get<Item>();
	}
    public float pullBack;
    bool canShoot = true;
	protected override void OnUpdate()
	{

        if(!item.mainHeld) return;

        pullBack = canShoot ? item.Controller.Trigger.Value * (1/PullBackShoot) : 0;
        
        if(item.Controller.Trigger.Value >= PullBackShoot && canShoot)
        {
            TriggerBones[multiBarrelMagazine.currentBarrel].Transform.LocalRotation = TargetTriggerRots[multiBarrelMagazine.currentBarrel];
            multiBarrelMagazine.LoadBarrel(Barrels[multiBarrelMagazine.currentBarrel]);
            Barrels[multiBarrelMagazine.currentBarrel].Fire();
            canShoot = false;
        }

        if(item.Controller.Trigger.Value <= 0.01f)
            canShoot = true;

        TriggerBones[multiBarrelMagazine.currentBarrel].Transform.LocalRotation = Angles.Lerp(TargetTriggerRots[multiBarrelMagazine.currentBarrel], TargetTriggerBackRots[multiBarrelMagazine.currentBarrel],pullBack);

        SetFingerPos(
            item.Controller == Input.VR.RightHand ? FingerRight : FingerLeft,
            TriggerSafeRef , 
            TriggerRefs[multiBarrelMagazine.currentBarrel], 
            TriggerBackRefs[multiBarrelMagazine.currentBarrel], 
            item, 
            item.Controller == Input.VR.RightHand ? Vector3.One : LeftFingerPosMod, 
            item.Controller == Input.VR.RightHand ? new Angles(1,1,1) : LeftFingerRotMod,
            pullBack,
            item.Controller.GetFingerValue(Sandbox.VR.FingerValue.IndexCurl) );
        
	}

    public static void SetFingerPos(GameObject finger, GameObject triggerSafe, GameObject trigger, GameObject triggerBack, Item item, Vector3 posMod, Angles angMod, float pullBack, float fingerValue)
    {
        if(fingerValue < 0.5f) 
        {
            HandsDealer.CopyTransformRecursive(triggerSafe, finger, posMod, angMod);
        }
        else
        {
            HandsDealer.CopyTransformRecursiveLerp(trigger, triggerBack, finger, posMod, angMod, pullBack);
        }
    }
}
