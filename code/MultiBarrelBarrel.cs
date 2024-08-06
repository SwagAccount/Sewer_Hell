using Sandbox;
using trollface;

public sealed class MultiBarrelBarrel : BarrelBase
{
	MultiBarrelMagazine multiBarrelMagazine;
	protected override void OnStart()
	{
		base.OnStart();
		multiBarrelMagazine = Components.Get<MultiBarrelMagazine>();
	}
	public override void Fire()
    {
        base.Fire();
        multiBarrelMagazine.Shoot();
    }
}
