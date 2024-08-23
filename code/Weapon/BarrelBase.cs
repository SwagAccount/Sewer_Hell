using System;

namespace trollface;
public abstract class BarrelBase : Component
{
	public Recoil Recoil {get;set;}
	[Property] public int Durability {get;set;} = 400;
	[Property] public int BarrelContent {get;set;} = -1;
	[Property] public float VelocityMultiplier {get;set;} = 1f;
	[Property] public Vector2 SpreadMult {get;set;} = Vector3.One;
	[Property] public GameObject BarrelEnd {get;set;}
	[Property] public SoundEvent GunShot {get;set;}
	[Property] public SoundEvent DryFire {get;set;}
	[Property] public GameObject MuzzleFlashPrefab {get;set;}
	BulletTypes bulletTypes;	 
	Rigidbody rigidbody;
	[Property] public Item item {get;set;}

	Vrmovement vrmovement;
	protected override void OnStart()
	{
		vrmovement = Scene.Components.GetInChildren<Vrmovement>();
		item = GameObject.Parent.Components.Get<Item>();
		bulletTypes = Components.Get<BulletTypes>();
		rigidbody = GameObject.Parent.Components.Get<Rigidbody>();
		Recoil = Components.Get<Recoil>();
	}
	Vector3 CalculateSpread(Bullet bullet)
	{
		return BarrelEnd.Transform.World.Forward + 
		(BarrelEnd.Transform.World.Right*(bullet.Spread.x*SpreadMult.x)*(Game.Random.Next(-100,100)/100f)) +
		(BarrelEnd.Transform.World.Up*(bullet.Spread.y*SpreadMult.y)*(Game.Random.Next(-100,100)/100f));
	}
	public bool hasFired;
	public virtual async void Fire()
	{
		hasFired = false;
		if(BarrelContent != -1 && BarrelContent != -2 && (Game.Random.Next(0,100)/100f) > MathF.Pow(1-item.Condition,3))
		{
			hasFired = true;
			item.Condition -= 1f/Durability;
			Log.Info(item.Condition);
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
				bulletProjectile.owner = vrmovement.characterController.GameObject;
				bulletProjectile.Firerer = GameObject.Parent;
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
			item.Condition -= 0.5f/Durability;
			Sound.Play(DryFire, Transform.Position);
		}
	}

}