namespace trollface;
public sealed class RevolverTrigger : Component
{
	[Property] public BarrelBase Barrel {get;set;}
	[Property] public RevolverCylinder RevolverCylinder {get;set;}
	[Property] public GameObject HammerBone {get;set;}
	[Property] public Angles TargetHammerClosedRot {get;set;}
	[Property] public Angles TargetHammerOpenRot {get;set;}
	[Property] public float pullBackAmount {get;set;} = 0.75f;
    
    public Item item;

	protected override void OnStart()
	{
		item = GameObject.Parent.Components.Get<Item>();
	}
    bool canFire = true;
	protected override void OnUpdate()
	{
        if(!item.held) return;
        
        if(canFire)
        {
            HammerBone.Transform.LocalRotation = Angles.Lerp(TargetHammerClosedRot,TargetHammerOpenRot,item.Controller.Trigger.Value);

            RevolverCylinder.RotateAmount = item.Controller.Trigger.Value * (1/pullBackAmount);

            if(item.Controller.Trigger.Value > pullBackAmount)
            {
                RevolverCylinder.LoadBarrel();
                Barrel.Fire();
                canFire = false;
            }
        }
        else
        {
            RevolverCylinder.RotateAmount = 0;
            HammerBone.Transform.LocalRotation = TargetHammerClosedRot;
        }

        if(item.Controller.Trigger.Value.AlmostEqual(0))
        {
            canFire = true;
        }
	}
}
