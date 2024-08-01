using Sandbox;
namespace trollface;
public sealed class RevolverBarrel : BarrelBase
{
	[Property] public RevolverCylinder RevolverCylinder {get;set;}

    public override void Fire()
    {
        base.Fire();
        RevolverCylinder.Shoot();
    }
}