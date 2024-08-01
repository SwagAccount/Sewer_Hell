namespace trollface;
public abstract class MagazineBase : Component
{
	[Property] public bool CantLoad {get;set;}
	[Property] public List<int> Contents {get;set;}
	[Property] public BarrelBase Barrel {get;set;}
	[Property] public BulletTypes bulletTypes {get;set;}
	[Property] public List<GameObject> Loaders {get;set;}
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
					Contents[i] = bulletTypes.Bullets.IndexOf(bullet);
					UpdateVisuals();
					g.Destroy();
				}
			}
		}
	}
	public virtual void LoadBarrel()
	{

	}

	public virtual void UpdateVisuals()
	{

	}

}