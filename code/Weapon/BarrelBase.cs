namespace trollface;
public abstract class BarrelBase : Component
{
	[Property] public Recoil Recoil {get;set;}
	[Property] public int BarrelContent {get;set;} = -1;
	[Property] public float VelocityMultiplier {get;set;} = 1f;
	[Property] public Vector2 Spread {get;set;}
	[Property] public GameObject BarrelEnd {get;set;}
	Rigidbody rigidbody;
	protected override void OnStart()
	{
		rigidbody = GameObject.Parent.Components.Get<Rigidbody>();
	}
	public virtual void Fire()
	{
		if(BarrelContent != -1)
			Recoil.ApplyRecoil();
		else
			Log.Info("Dry Fire");
	}

}