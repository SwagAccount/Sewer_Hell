using System;
using Sandbox;
namespace trollface;
public sealed class FindChooseEnemy : Component
{
	[Property] public GameObject RelativeGameObject {get;set;}
	[Property] public GameObject Enemy {get;set;}
	[Property] public AgroRelations EnemyRelations {get;set;}
	[Property] public HealthComponent HealthComponent {get;set;}
	[Property] public bool NewEnemy {get;set;}
	[Property] public float TimeSinceSeen {get;set;}
	[Property] public float DetectRange {get;set;} = 700f;
	[Property] public float ForceTargetRange {get;set;} = 300f;
	[Property] public Vector3 eyePos {get;set;}
	[Property] public Vector3 eyeDir {get;set;} = new Vector3(1,0,0);
	[Property] public float ViewAngle {get;set;}

	AgroRelations agroRelations;

	protected override void DrawGizmos()
	{
		Vector3 eyePosL = Transform.World.PointToLocal(RelativeGameObject.Transform.World.PointToWorld(eyePos));
		Vector3 eyeDirL = Transform.World.PointToLocal(RelativeGameObject.Transform.World.PointToWorld(eyeDir));
		Gizmo.Draw.Arrow(eyePosL,eyePosL+eyeDirL,1,1);
		var baseRotation = Rotation.LookAt(eyeDirL);

		var axes = new[] { Vector3.Up, Vector3.Right };
		var angles = new[] { ViewAngle, -ViewAngle };

		foreach (var axis in axes)
		{
			foreach (var angle in angles)
			{
				var rotatedDirection = baseRotation.RotateAroundAxis(axis, angle).Forward * 50;

				Gizmo.Draw.Line(eyePos, eyePos + rotatedDirection);
			}
		}
	}
	protected override void OnStart()
	{
		if(RelativeGameObject == null) RelativeGameObject = GameObject;
		agroRelations = Components.GetOrCreate<AgroRelations>();
		HealthComponent = Components.GetOrCreate<HealthComponent>();

		if(agroRelations.Faction == null || agroRelations.Enemies == null)
		{
			agroRelations.Faction = "Enemy";
			agroRelations.Enemies = new List<string>{"Player"};
		}
	}
	GameObject lastAttacker = null;
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
			GameObject hitObject = Scene.Trace.Ray(RelativeGameObject.Transform.World.PointToWorld(eyePos), gAgroRelations.Transform.World.PointToWorld(gAgroRelations.attackPoint)).WithAnyTags("world","player").UseHitboxes().IgnoreGameObjectHierarchy(GameObject).Run().GameObject;
			if(hitObject != gAgroRelations.ObjectRef) continue;

			Transform transform = RelativeGameObject.Transform.World;
			transform.Position = Vector3.Zero;
			Vector3 direction = transform.PointToWorld(eyeDir);


			if(MathF.Abs(Vector3.GetAngle(direction,gAgroRelations.Transform.World.PointToWorld(gAgroRelations.attackPoint)-RelativeGameObject.Transform.World.PointToWorld(eyePos))) > ViewAngle) continue;
			float distance = Vector3.DistanceBetween(g.Transform.Position,Transform.Position);
			if(distance < closestRange)
			{
				TimeSinceSeen = 0;
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
			return;
		}


		if(closestRange < ForceTargetRange && Enemy != closest)
		{
			Enemy = closest;
			EnemyRelations = closestRelations;
			NewEnemy = true;
		}
	}

}
