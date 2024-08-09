using Sandbox;
namespace trollface;
public sealed class FindChooseEnemy : Component
{
	[Property] public GameObject Enemy {get;set;}
	[Property] public AgroRelations EnemyRelations {get;set;}
	[Property] public HealthComponent HealthComponent {get;set;}
	[Property] public bool NewEnemy {get;set;}
	[Property] public float TimeSinceSeen {get;set;}
	[Property] public float DetectRange {get;set;} = 700f;
	[Property] public float ForceTargetRange {get;set;} = 300f;

	AgroRelations agroRelations;

	protected override void OnStart()
	{
		agroRelations = Components.GetOrCreate<AgroRelations>();
		HealthComponent = Components.GetOrCreate<HealthComponent>();

		if(agroRelations.Faction == null || agroRelations.Enemies == null)
		{
			agroRelations.Faction = "Enemy";
			agroRelations.Enemies = new List<string>{"Player"};
		}
	}
	GameObject lastAttacker;
	protected override void OnFixedUpdate()
	{
		(bool isTrue, AgroRelations agroRelations) isEnemy(GameObject g)
		{
			if(g.Tags == null) return (false,null);	

			if(g == GameObject) return (false,null);
			
			if(!g.Tags.Contains("relations")) return (false,null);
			
			AgroRelations gAgroRelations = g.Components.Get<AgroRelations>();
			
			if(gAgroRelations ==null) return (false, null);
			
			if(!agroRelations.Enemies.Contains(gAgroRelations.Faction))
			{
				return (false,gAgroRelations);
			} 

			return (true, gAgroRelations);
		}

		
		
		if(lastAttacker != HealthComponent.lastAttacker)
		{
			GameObject g = HealthComponent.lastAttacker;
			
			(bool isTrue, AgroRelations gAgroRelations) = isEnemy(g);
			
			
			if(!agroRelations.Enemies.Contains(gAgroRelations.Faction) && gAgroRelations.Faction != agroRelations.Faction)
			{
				isTrue = true;
				agroRelations.Enemies.Add(gAgroRelations.Faction);
			}

			if(isTrue)
			{
				Enemy = g;
				EnemyRelations = gAgroRelations;
				return;
			}
		}
		

		TimeSinceSeen+=Time.Delta;

		List<GameObject> Detected = Scene.FindInPhysics(new Sphere(Transform.Position,DetectRange)).ToList();
		if (Detected == null || Detected.Count() < 1) return;
		GameObject closest = null;
		AgroRelations closestRelations = null;
		float closestRange = DetectRange;
		foreach(GameObject g in Detected)
		{
			
			(bool isTrue, AgroRelations gAgroRelations) = isEnemy(g);
			if(!isTrue) continue;	
			
			float distance = Vector3.DistanceBetween(g.Transform.Position,Transform.Position);
			if(distance < closestRange)
			{
				closest = g;
				closestRelations = gAgroRelations;
				closestRange = distance;
			}
		}
		
		if (closest == null) return;

		if(!Enemy.IsValid())
		{
			Enemy = closest;
			EnemyRelations = closestRelations;
			NewEnemy = true;
			TimeSinceSeen = 0;
			return;
		}


		if(closestRange < ForceTargetRange && Enemy != closest)
		{
			Enemy = closest;
			EnemyRelations = closestRelations;
			NewEnemy = true;
			TimeSinceSeen = 0;
		}
	}

}
