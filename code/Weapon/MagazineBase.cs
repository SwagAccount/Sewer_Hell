namespace trollface;
public abstract class MagazineBase : Component
{
	
	[Property] public bool CantLoad {get;set;}
	
	[Property] public int MagSize {get;set;}
	[Property] public List<int> Contents {get;set;}
	[Property] public BarrelBase Barrel {get;set;}
	[Property] public BulletTypes bulletTypes {get;set;}
	[Property] public List<GameObject> Loaders {get;set;}
	[Property] public SoundEvent LoadSound {get;set;}
	[Property] public List<GameObject> BulletVisuals {get;set;}

	[Property] public bool ActualMag {get;set;} = true;
	[Property] public List<string> notMagAccepted {get;set;}
	[Property] public List<GameObject> notMagPrefabs {get;set;}
	[Property] public Item item {get;set;}
	[Property] public bool PushBack {get;set;}

	public GameObject GetPrefab(int i)
	{
		if(ActualMag)
		{
			return bulletTypes.Bullets[Contents[i]].PrefabRef;
		}
		else
		{
			return notMagPrefabs[Contents[i]];
		}
	}
	public bool fuckyou;
	protected override void OnStart()
	{
		item = Components.GetInParentOrSelf<Item>();
	}

	protected override void OnFixedUpdate()
	{
		
		if(!Contents.Contains(-1) && !PushBack) return;
		
		for(int i = 0; i < Loaders.Count; i++)
		{
			
			if(!PushBack && Contents[i] != -1) continue;

			IEnumerable<GameObject> gameObjects = Scene.FindInPhysics(new Sphere(Loaders[i].Transform.Position,0.25f));
			foreach(GameObject g in gameObjects)
			{
				
				Item item = g.Components.Get<Item>();
				if(item == null) return;
				if(ActualMag)
				{
					Bullet bullet = ResourceLibrary.Get<Bullet>($"bullets/{item.ItemName}.bullet");
					if(bulletTypes.Bullets.Contains(bullet))
					{
						AddContent(g, i, bulletTypes.Bullets.IndexOf(bullet));
						break;
					}
				}
				else
				{
					if(notMagAccepted.Contains(item.ItemName))
					{
						AddContent(g, i , notMagAccepted.IndexOf(item.ItemName));
						break;
					}
				}
			}
		}
	}

	void AddContent(GameObject g, int index, int bullet)
	{
		
		if(!PushBack)
		{
			Contents[index] = bullet;
		}
		else
		{
			
			if(Contents.Count >= MagSize) return;
			
			Contents.Insert(0, bullet);
		}
		UpdateVisuals();
		g.DestroyImmediate();
		if(LoadSound != null) Sound.Play(LoadSound, Loaders[index].Transform.Position );
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
			if(i < Contents.Count)
				BulletVisuals[i].Enabled = Contents[i] != -1;
			else
				BulletVisuals[i].Enabled = false;
		}
	}

}