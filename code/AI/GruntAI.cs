using Sandbox;
using System;
using Sandbox.Navigation;
using trollface;

public sealed class GruntAI : AIAgent
{
    public CoverFinder CoverFinder;
    public EnemyWeaponDealer EnemyWeaponDealer;
    public FindChooseEnemy FindChooseEnemy;
    [Property] public ModelPhysics Body {get;set;}
    [Property] public AnimationHelper AnimationHelper {get;set;}
    //public PlayerBody Body {get;set;}
    AgroRelations agroRelations;
    HealthComponent healthComponent;
    float lastHealth;
    //public int Grenades {get;set;} = 3;
    //public float GrenadeTime {get;set;} = 30f;
    //public float GrenadeRange {get;set;} = 250f;
    [Property] public Vector3 EyePos {get;set;}
    [Property] public float SuppressPatience {get;set;} = 15f;
    [Property] public float Patience {get;set;} = 30f;


    bool distanceCalculated;
    [Property] public float RandomMoveTime {get;set;} = 10f;
    [Property] public Vector2 RandomMoveDis {get;set;} = new Vector2(50,100);
    [Property] public float CoverSensitivity {get;set;} = 500f;
    [Property] public float CoverCheckDistance {get;set;} = 500f;
    [Property] public float WalkSpeed {get;set;} = 450f;
    [Property] public float SprintingSpeed {get;set;} = 450f;
    [Property] public float AttackDistance {get;set;} = 450f;
    [Property] public float MaxAttackDistance {get;set;} = 700f;
    [Property] public float MajorDamageAmount {get;set;} = 20f;
    [Property] public float HealthMemory {get;set;} = 0.5f;
    [Property] public bool IsCrouching { get; set; }
    [Property] public float SmoothCrouchSpeed { get; set; } = 5.0f;
    [Property] public float HideTime { get; set; } = 5.0f;
    [Property] public float MaxDistanceFromCover { get; set; } = 150f;
    [Property] public float RotateSpeed { get; set; } = 0.01f;

    CoverContext currentCover;

    private float smoothCrouch;
    //float currentGrenadeTime;

    protected override void SetStates()
    {
        //currentGrenadeTime = GrenadeTime;
        //Body = AnimationHelper.Components.Get<PlayerBody>();
        
        healthComponent = Components.Get<HealthComponent>();
        CoverFinder = Scene.Components.GetInChildren<CoverFinder>();
        FindChooseEnemy = Components.GetOrCreate<FindChooseEnemy>();
        EyePos = FindChooseEnemy.eyePos;
        agroRelations = Components.Get<AgroRelations>();
        EnemyWeaponDealer = Components.Get<EnemyWeaponDealer>();
        initialState = "IDLE";
        stateMachine.RegisterState(new SUPPRESS());
        stateMachine.RegisterState(new PRESS_ATTACK());
        stateMachine.RegisterState(new FIRE_DISTANCE());
        stateMachine.RegisterState(new COVER());
        stateMachine.RegisterState(new HIDE_AND_RELOAD());
        stateMachine.RegisterState(new CAUTION_COVER());
        stateMachine.RegisterState(new IDLE());
        lastHealth = healthComponent.Health;
    }

	private void Die()
	{
        Body.Enabled = true;
        chunkDealer.PlaceInChunk(Body.GameObject);
        GameObject.Destroy();

	}

    public void Animate(Vector3 velocity, bool isOnGround, Vector3 lookDirection, float crouchLevel, AnimationHelper.HoldTypes holdType, AnimationHelper.Hand hand, bool loweredWeapon)
    {
        AnimationHelper.WithVelocity( velocity.IsNearlyZero() ? Vector3.Zero : velocity );
		AnimationHelper.IsGrounded = isOnGround;
		AnimationHelper.WithLook( lookDirection, 1, 1, 1.0f );
		AnimationHelper.MoveStyle = AnimationHelper.MoveStyles.Run;
		AnimationHelper.DuckLevel = crouchLevel;
		AnimationHelper.HoldType = holdType;
		AnimationHelper.Handedness = hand;
		AnimationHelper.AimBodyWeight = 0.1f;
        AnimationHelper.Target.Set("Aim" , !loweredWeapon);
    }

    protected override void Update()
    {
        if(!Networking.IsHost) return;
        if(healthComponent.Health <= 0)
        {
            Die();
            return;
        }
        
        //currentGrenadeTime+=Time.Delta;
        
        float targetCrouch = IsCrouching ? 1.5f : 0.0f;
        smoothCrouch = MathX.Lerp(smoothCrouch, targetCrouch, Time.Delta * SmoothCrouchSpeed);
        
        Agent.MaxSpeed = FindChooseEnemy.Enemy.IsValid() ? SprintingSpeed*0.75f : WalkSpeed*0.75f;
        if ( AnimationHelper.IsValid() )
		{
            Animate(
                Agent.Velocity.IsNearlyZero() ? Vector3.Zero : Agent.Velocity,
                true,
                FindChooseEnemy.Enemy != null ? (FindChooseEnemy.Enemy.Transform.World.PointToWorld(FindChooseEnemy.EnemyRelations.attackPoint) - Transform.World.PointToWorld(EyePos)) : Transform.World.Forward,
                smoothCrouch,
                EnemyWeaponDealer.HoldType,
                EnemyWeaponDealer.Handedness,
                FindChooseEnemy.Enemy == null
            );
		}
        
        if(FindChooseEnemy.Enemy.IsValid())
        {
            stateMachine.ChangeState(FindCondition());
            FaceThing(FindChooseEnemy.Enemy, Body.GameObject);
        }
        else
        {
            stateMachine.ChangeState("IDLE");
        }

        lastHealth = MathX.Lerp(lastHealth,healthComponent.Health,Time.Delta*HealthMemory);
        EnemyWeaponDealer.Shooting = false;
        EnemyWeaponDealer.Shooting = false;
        
    }
    
    public void FaceThing(GameObject thing, GameObject faced = null)
    {
        if(faced == null) faced = GameObject;
        Angles angles = Rotation.LookAt(thing.Transform.Position - Transform.Position);
        angles.pitch = 0;
        angles.roll = 0;
        faced.Transform.Rotation = Rotation.Lerp(faced.Transform.Rotation, angles, RotateSpeed * Time.Delta);
    }

    public (bool found, Vector3 coverPoint, float distance) CheckCover()
    {
        List<SceneTraceResult> checkPoints = RayPointsInRadius(Transform.Position+Vector3.Up*5, CoverCheckDistance, 10);
        
        foreach(SceneTraceResult result in checkPoints)
        {
            Vector3 point = result.HitPosition;
            bool findOtherSide = false;
            int maxIterations = 2;
            int iterationCount = 0;
            while(iterationCount < maxIterations)
            {
                
                Vector3? closestEdge = Scene.NavMesh.GetClosestPoint(
                    !findOtherSide ? point : point + (point - Transform.Position).Normal*50f
                );

                if(!closestEdge.HasValue)
                {
                    if(findOtherSide)
                        break;
                    findOtherSide = true;
                    continue;
                }
                
                if(Vector3.Dot(result.Normal, (FindChooseEnemy.Enemy.Transform.Position - closestEdge.Value).Normal) < CoverSensitivity)
                    return(true, closestEdge.Value, Vector3.DistanceBetween(closestEdge.Value,FindChooseEnemy.Enemy.Transform.Position));

                iterationCount++;
            }
        }
        return(false, Vector3.Zero, 0);
    }

    List<SceneTraceResult> RayPointsInRadius(Vector3 origin, float radius, float increment)
    {
        List<SceneTraceResult> hitPoints = new List<SceneTraceResult>();

        int numRays = (int)MathF.Round(360f / increment);

        for (int i = 0; i < numRays; i++)
        {
            float angle = i * increment;
            Vector3 direction = Rotation.FromAxis(Vector3.Up, angle) * Vector3.Forward;

            Vector3 end = origin + direction * radius;
            var hit = Scene.Trace.Ray(origin, end).WithTag("world").Run();

            if (hit.Hit)
            {
                hitPoints.Add(hit);
            }
        }

        return hitPoints;
    }



    public bool RetreatToFireLine()
    {
        return EnemyDistance() < AttackDistance;
    }

    public bool OutsideMaxAttack()
    {
        return EnemyDistance() > MaxAttackDistance;
    }

    public float EnemyDistance()
    {
        return Vector3.DistanceBetween(Transform.Position,FindChooseEnemy.Enemy.Transform.Position);
    }

    public void ClaimCover(CoverContext coverContext)
    {
        coverContext.owned = true;
        currentCover = coverContext;
    }

    public void DropCover()
    {
        currentCover.owned = false;
        currentCover = null;
    }

    public void CanSeeEnemy()
    {

    }

    string FindCondition()
    {
        if (stateMachine.currentState == "IDLE")
        {
            GameObject pEnemy = FindChooseEnemy.Enemy;
            //float flTimeSinceFirstSeen = FindChooseEnemy.TimeSinceSeen; // Assuming Time.Now is current time

            //if (flTimeSinceFirstSeen < 3.0f)
                //bFirstContact = true;

            if ( pEnemy != null)
            {
                if (Game.Random.Next(0, 100) < 60) 
                {
                    return "FIRE_DISTANCE";
                }
                else
                {
                    return "PRESS_ATTACK";
                }
            }
        }

        if (EnemyWeaponDealer.Ammo == 0 || EnemyWeaponDealer.Ammo < EnemyWeaponDealer.Ammo/10)
        {
            return "HIDE_AND_RELOAD";
        }
        else if (stateMachine.currentState == "HIDE_AND_RELOAD")
        {
            return "COVER" ;
        }

        
        if (lastHealth-healthComponent.Health > MajorDamageAmount)
        {
            return "CAUTION_COVER";
        }
        
        if (MathF.Abs(EnemyDistance() - AttackDistance) < 50)
        {
            return "COVER";
        }

        // Default return, if no conditions matched
        return stateMachine.currentState;
    }

}
/*
"SUPPRESS"
"FIRE_DISTANCE"
"PRESS_ATTACK"
"COVER"
"HIDE_AND_RELOAD"
"CAUTION_COVER"
*/
public class IDLE : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
        agent.Agent.MoveTo(agent.Transform.Position); 
        agent.chunkDealer.PlaceInChunk(agent.GameObject);
	}

	public void Exit( AIAgent agent )
	{
		
	}

	public string GetID()
	{
		return "IDLE";
	}
	
    float timeSinceLastCanShoot;
	public void Update( AIAgent agent )
	{
        float chance = Time.Delta / gruntAI.RandomMoveTime;
        if(Game.Random.Next(0,100000)/100000f < chance)
        {
            float distanceMod = Game.Random.Next(0,100)/100;
            float distance = (distanceMod * (gruntAI.RandomMoveDis.y-gruntAI.RandomMoveDis.x))+gruntAI.RandomMoveDis.x;
            agent.Agent.MoveTo(agent.Transform.Position+(Vector3.Random.WithZ(0)*distance));
        }
	}
}
public class SUPPRESS : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
        gruntAI.IsCrouching = false;
	}

	public void Exit( AIAgent agent )
	{

	}

	public string GetID()
	{
		return "SUPPRESS";
	}
	
    float timeSinceLastCanShoot;
	public void Update( AIAgent agent )
	{
        timeSinceLastCanShoot++;
        agent.Agent.MoveTo(agent.Transform.Position);
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy, gruntAI.FindChooseEnemy.Enemy.Transform.World.PointToWorld(gruntAI.FindChooseEnemy.EnemyRelations.attackPoint)))
        {

            timeSinceLastCanShoot = 0;
            gruntAI.EnemyWeaponDealer.Shooting = true;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Shooting = false;
            if(timeSinceLastCanShoot > gruntAI.SuppressPatience) agent.Agent.MoveTo(gruntAI.FindChooseEnemy.Enemy.Transform.Position);
        }
	}
}
public class PRESS_ATTACK : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
        gruntAI.IsCrouching = false;
	}

	public void Exit( AIAgent agent )
	{

	}

	public string GetID()
	{
		return "PRESS_ATTACK";
	}
	
	public void Update( AIAgent agent )
	{
        agent.Agent.MoveTo(gruntAI.EnemyDistance() > 100 ? gruntAI.FindChooseEnemy.Enemy.Transform.Position : agent.Transform.Position);
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy, gruntAI.FindChooseEnemy.Enemy.Transform.World.PointToWorld(gruntAI.FindChooseEnemy.EnemyRelations.attackPoint)))
        {
            gruntAI.EnemyWeaponDealer.Shooting = true;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Shooting = false;
        }
	}
}
public class FIRE_DISTANCE : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
        gruntAI.IsCrouching = false;
	}

	public void Exit( AIAgent agent )
	{
	}

	public string GetID()
	{
		return "FIRE_DISTANCE";
	}
	
	public void Update( AIAgent agent )
	{

		gruntAI.EnemyWeaponDealer.Shooting = gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy, gruntAI.FindChooseEnemy.Enemy.Transform.World.PointToWorld(gruntAI.FindChooseEnemy.EnemyRelations.attackPoint));

        
        if(gruntAI.RetreatToFireLine()) 
        {
            agent.Agent.MoveTo(agent.Transform.Position);
        }
        else if (gruntAI.OutsideMaxAttack())
        {
            agent.Agent.MoveTo(gruntAI.FindChooseEnemy.Enemy.Transform.Position);
        }
        else
        {
            agent.stateMachine.ChangeState("SUPPRESS");
        }
	}
}
public class COVER : AIState
{
    GruntAI gruntAI;

    float coverValue = 0.5f;
    float patience;
	public void Enter( AIAgent agent )
	{
        patience = 0;
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{
	}

	public string GetID()
	{
		return "COVER";
	}
    bool moveOutOfCover = true;
	public void Update( AIAgent agent )
	{
        (bool found, Vector3 coverPoint, float distance) = gruntAI.CheckCover();

        patience+=Time.Delta;
        GameObject enemy = gruntAI.FindChooseEnemy.Enemy;
        bool unCover = found ? Sandbox.Utility.Noise.Perlin(Time.Now*20, coverPoint.Length) > 0.5f : false;
        
        

        
        
        if (!found)
        {
            if (gruntAI.RetreatToFireLine())
            {
                agent.Agent.MoveTo( gruntAI.Transform.Position - (enemy.Transform.Position - gruntAI.Transform.Position).Normal * 150f);
            }
            else if (gruntAI.OutsideMaxAttack())
            {
                agent.Agent.MoveTo( gruntAI.FindChooseEnemy.Enemy.Transform.Position);
            }
            else
            {
                agent.Agent.MoveTo( gruntAI.Transform.Position);
            }
        }
        else
        {
            if (false)
            {
                agent.Agent.MoveTo( coverPoint );
            }
            else
            {
                if (unCover)
                {
                    if(moveOutOfCover)
                        agent.Agent.MoveTo( distance < gruntAI.MaxDistanceFromCover ? enemy.Transform.Position : agent.Transform.Position);
                    else
                        agent.Agent.MoveTo( coverPoint);
                }
                else
                {
                    moveOutOfCover = true;
                    agent.Agent.MoveTo( coverPoint);
                }
            }
        }
        
        
        if(found)
            gruntAI.IsCrouching = unCover && (distance < 20);//currentCover.wall ? false : unCover && (distance < 20);
        else
            gruntAI.IsCrouching = false;
        
        
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy, gruntAI.FindChooseEnemy.Enemy.Transform.World.PointToWorld(gruntAI.FindChooseEnemy.EnemyRelations.attackPoint)))
        {
            moveOutOfCover = false;
            gruntAI.EnemyWeaponDealer.Shooting= true;
            patience = 0;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Shooting = false;
        }

        if(patience > gruntAI.Patience) agent.stateMachine.ChangeState("SUPPRESS");
        
	}
    
}
public class HIDE_AND_RELOAD : AIState
{
    GruntAI gruntAI;


	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{
		gruntAI.EnemyWeaponDealer.Shooting = false;
	}

	public string GetID()
	{
		return "HIDE_AND_RELOAD";
	}
	public void Update( AIAgent agent )
	{
        GameObject enemy = gruntAI.FindChooseEnemy.Enemy;

        (bool found, Vector3 coverPoint, float distance) = gruntAI.CheckCover();
        Log.Info(found);
        agent.Agent.MoveTo
        (

            !found ? 
                gruntAI.Transform.Position - (enemy.Transform.Position - gruntAI.Transform.Position).Normal*150f
                :
                coverPoint
            
        );

        gruntAI.IsCrouching = distance < 20;
        
        gruntAI.EnemyWeaponDealer.Reloading = true;
	}
}
public class CAUTION_COVER : AIState
{
    GruntAI gruntAI;


	public void Enter( AIAgent agent )
	{
        hideTime = 0;
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{

	}

	public string GetID()
	{
		return "CAUTION_COVER";
	}
    float hideTime;
	public void Update( AIAgent agent )
	{
        hideTime += Time.Delta;
        
        GameObject enemy = gruntAI.FindChooseEnemy.Enemy;

        

        (bool found, Vector3 coverPoint, float distance) = gruntAI.CheckCover();
        agent.Agent.MoveTo
        (

            !found ? 
                gruntAI.Transform.Position - (enemy.Transform.Position - gruntAI.Transform.Position).Normal*150f
                :
                coverPoint
            
        );

        gruntAI.IsCrouching = distance < 20;
        
        
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy, gruntAI.FindChooseEnemy.Enemy.Transform.World.PointToWorld(gruntAI.FindChooseEnemy.EnemyRelations.attackPoint)))
        {
            gruntAI.EnemyWeaponDealer.Shooting = true;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Shooting = false;
        }

        if(hideTime>gruntAI.HideTime) agent.stateMachine.ChangeState("COVER");
	}

    
    
}