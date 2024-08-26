using Sandbox;
using System;
using trollface;

public sealed class EnemyWeaponDealer : Component
{
	[Property] public SkinnedModelRenderer skinnedModelRenderer {get;set;}
	[Property] public FindChooseEnemy findChooseEnemy {get;set;}
	[Property] public string ShootProperty {get;set;} = "b_attack";
	[Property] public SoundEvent GunShot {get;set;}
	[Property] public GameObject MuzzleFlashPrefab {get;set;}
	[Property] public GameObject Muzzle {get;set;}
	[Property] public Vector2 SpreadMult {get;set;} = Vector2.One;
	[Property] public float VelocityMultiplier {get;set;} = 1f;
	[Property] public Bullet bullet {get;set;}
	[Property] public AnimationHelper.Hand Handedness {get;set;}
	[Property] public AnimationHelper.HoldTypes HoldType {get;set;}
	[Property] public float FireRate {get;set;}
	[Property] public bool Shooting {get;set;}
	[Property] public bool Reloading {get;set;}
	[Property] public float ReloadTime {get;set;}
	[Property] public int Ammo {get;set;}
	[Property] public int maxAmmo {get;set;}
	[Property] public int HitChance {get;set;} = 30;

	public bool WeaponHitsTarget(GameObject target, Vector3 point)
	{
		var trace = Scene.Trace.Ray(Transform.World.PointToWorld(findChooseEnemy.eyePos),point).IgnoreGameObjectHierarchy(GameObject).WithoutTags("nocollide").UseHitboxes().Run();

		if(!trace.Hit) return false;
		return trace.GameObject == target || trace.GameObject.IsAncestor(target) || trace.GameObject.IsDescendant(target);
	}
	float LastFire;
	bool isReloading;
	async void Reload()
	{
		if(isReloading || Ammo == maxAmmo) return;
		isReloading = true;
		await Task.DelaySeconds(ReloadTime);
		Ammo = maxAmmo;
		isReloading = false;
	}
	protected override void OnUpdate()
	{
		if(Shooting && !isReloading)
		{
			if(Time.Now - LastFire > FireRate && Ammo > 0)
				Fire();
		}

		if(Reloading)
			Reload();
	}
	Vector3 CalculateSpread(Bullet bullet, Vector3 dir)
	{
		Vector3 projection = Vector3.Dot(Vector3.Up, dir) * dir;

		Vector3 up = (Vector3.Up - projection).Normal;

		return dir + 
		(Vector3.Cross(up,dir)*(bullet.Spread.x*SpreadMult.x)*(Game.Random.Next(-100,100)/100f)) +
		(Vector3.Up*(bullet.Spread.y*SpreadMult.y)*(Game.Random.Next(-100,100)/100f));
	}
	async void Fire()
	{
		if(Ammo <= 0) return;
		Ammo--;
		LastFire = Time.Now;
		skinnedModelRenderer.Set(ShootProperty, true);
		for (int i = 0; i < bullet.Count; i++)
		{
			var dir = 
				Game.Random.Next(0,100) < HitChance ? 
					(findChooseEnemy.Enemy.Transform.World.PointToWorld(findChooseEnemy.EnemyRelations.attackPoint)-Muzzle.Transform.Position).Normal
					:
					Muzzle.Transform.World.Forward
					;
			GameObject bulletObject = new GameObject();
			bulletObject.Transform.Position = Muzzle.Transform.Position;
			bulletObject.Transform.Rotation = Rotation.LookAt(dir);// Muzzle.Transform.Rotation;
			Rigidbody bulletBody = bulletObject.Components.Create<Rigidbody>();
			bulletBody.Velocity = dir * VelocityMultiplier * bullet.BaseVelocity * 12;
			BulletProjectile bulletProjectile = bulletObject.Components.Create<BulletProjectile>();
			bulletProjectile.owner = GameObject;
			bulletProjectile.Firerer = GameObject;
			bulletProjectile.bullet = bullet;
		}
		Sound.Play(GunShot, Muzzle.Transform.Position);
		GameObject flash = MuzzleFlashPrefab.Clone( new CloneConfig()
		{
			Parent = Muzzle,
			Transform = new(),
			StartEnabled = true
		} );
		await Task.DelaySeconds(0.1f);
		flash.Destroy();
	}
}
