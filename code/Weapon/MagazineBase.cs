namespace trollface;
public abstract class MagazineBase : Component
{
	[Property] public bool CantLoad {get;set;}
	[Property] public bool CantEject {get;set;}
	[Property] public List<int> Contents {get;set;}
	[Property] public BarrelBase Barrel {get;set;}
	[Property] public BulletTypes bulletTypes {get;set;}
	[Property] public List<GameObject> Loaders {get;set;}
	[Property] public SoundEvent LoadSound {get;set;}
	[Property] public List<GameObject> BulletVisuals {get;set;}
	public bool PushBack {get;set;}

	protected override void OnFixedUpdate()
	{
		if(!Contents.Contains(-1)) return;
		for(int i = 0; i < Loaders.Count; i++)
		{
			if(!PushBack && Contents[i] != -1) continue;

			IEnumerable<GameObject> gameObjects = Scene.FindInPhysics(new Sphere(Loaders[i].Transform.Position,0.25f));
			foreach(GameObject g in gameObjects)
			{
				Item item = g.Components.Get<Item>();
				if(item == null) return;

				Bullet bullet = ResourceLibrary.Get<Bullet>($"bullets/{item.ItemName}.bullet");
				if(bulletTypes.Bullets.Contains(bullet))
				{
					Contents[PushBack ? 0 : i] = bulletTypes.Bullets.IndexOf(bullet);
					UpdateVisuals();
					g.DestroyImmediate();
					Sound.Play(LoadSound, Loaders[i].Transform.Position );
					break;
				}
			}
		}
	}
	public virtual void LoadBarrel()
	{

	}

	public void DropCases(SoundEvent EjectSound, GameObject Case)
	{
		bool soundPlayed = false;
		for(int i = 0; i < BulletVisuals.Count(); i++)
		{
			if(Contents[i] != -2) continue;
			if(!soundPlayed) Sound.Play(EjectSound,BulletVisuals[i].Transform.Position).Pitch = Game.Random.Next(90,110)/100;
			soundPlayed = true;
			GameObject newCase = Case.Clone();
			newCase.SetParent(BulletVisuals[i].Parent);
			newCase.Transform.Position = BulletVisuals[i].Transform.Position;
			newCase.Transform.Rotation = BulletVisuals[i].Transform.Rotation;
			Contents[i] = -1;
		}
	}

	public virtual void UpdateVisuals()
	{
		for(int i = 0; i < BulletVisuals.Count(); i++)
		{
			BulletVisuals[i].Enabled = Contents[i] != -1;
		}
	}

}