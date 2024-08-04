namespace trollface;
public abstract class BarrelBase : Component
{
	[Property] public Recoil Recoil {get;set;}
	[Property] public int BarrelContent {get;set;} = -1;
	[Property] public float VelocityMultiplier {get;set;} = 1f;
	[Property] public Vector2 Spread {get;set;}
	[Property] public GameObject BarrelEnd {get;set;}
	[Property] public SoundEvent GunShot {get;set;}
	[Property] public SoundEvent DryFire {get;set;}
	[Property] public GameObject MuzzleFlashPrefab {get;set;}
	BulletTypes bulletTypes;
	Rigidbody rigidbody;
	Item item;
	protected override void OnStart()
	{
		item = GameObject.Parent.Components.Get<Item>();
		bulletTypes = Components.Get<BulletTypes>();
		rigidbody = GameObject.Parent.Components.Get<Rigidbody>();
	}
	Vector3 CalculateSpread(Bullet bullet)
	{
		return BarrelEnd.Transform.World.Forward + 
		(BarrelEnd.Transform.World.Right*(bullet.Spread.x+Spread.x)*(Game.Random.Next(-100,100)/100)) +
		(BarrelEnd.Transform.World.Up*(bullet.Spread.y+Spread.y)*(Game.Random.Next(-100,100)/100));
	}
	public virtual async void Fire()
	{
		if(BarrelContent != -1)
		{
			Recoil.ApplyRecoil();
			item.Controller.TriggerHapticVibration(1,1,1);
			Bullet bullet = bulletTypes.Bullets[BarrelContent];
			for (int i = 0; i < bullet.Count; i++)
			{
				GameObject bulletObject = new GameObject();
				bulletObject.Transform.Position = BarrelEnd.Transform.Position;
				bulletObject.Transform.Rotation = BarrelEnd.Transform.Rotation;
				Rigidbody bulletBody = bulletObject.Components.Create<Rigidbody>();
				bulletBody.Velocity = CalculateSpread(bullet) * VelocityMultiplier * bullet.BaseVelocity * 12;
				BulletProjectile bulletProjectile = bulletObject.Components.Create<BulletProjectile>();
				bulletProjectile.bullet = bullet;
			}
			Sound.Play(GunShot, BarrelEnd.Transform.Position);
			GameObject flash = MuzzleFlashPrefab.Clone( new CloneConfig()
			{
				Parent = BarrelEnd,
				Transform = new(),
				StartEnabled = true
			} );
			await Task.DelaySeconds(0.1f);
			flash.Destroy();
		}
		else
		{
			Sound.Play(DryFire, Transform.Position);
		}
	}

}