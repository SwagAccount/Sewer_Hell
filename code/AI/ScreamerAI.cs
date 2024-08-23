using Sandbox;
using Sandbox.Navigation;
using System;
using System.Threading.Tasks;
namespace trollface;
public sealed class ScreamerAI : AIAgent
{
    [Property] public SoundEvent ScreamSound {get; set;}
    [Property] public GameObject HeadBones {get; set;}
    [Property] public SkinnedModelRenderer Body {get; set;}
    [Property] public BodyDeath BodyDeath {get; set;}
    [Property] public float LooseTime {get;set;} = 20f;
    [Property] public float RotateSpeed {get;set;} = 10f;
    [Property] public float ScreamDistance {get;set;} = 500f;
    [Property] public float ScreamTime {get;set;} = 30f;
    [Property] public float TryScreamTime {get;set;} = 7f;
    [Property] public float AttackDistance {get;set;} = 30f;
    [Property] public float StopDistance {get;set;} = 10f;
    [Property] public float AttackTime {get;set;} = 0.28f;
    [Property] public float RandomMoveTime {get;set;} = 10f;
    [Property] public Vector2 RandomMoveDis {get;set;} = new Vector2(50,100);
    public FindChooseEnemy FindChooseEnemy;
    HealthComponent healthComponent;

    Vrmovement player;
    protected override void SetStates()
    {
        player = Scene.Components.GetInChildren<Vrmovement>();
        healthComponent = Components.Get<HealthComponent>();
        FindChooseEnemy = Components.Get<FindChooseEnemy>();
        initialState = "IDLE";
        stateMachine.RegisterState(new IDLE());
        stateMachine.RegisterState(new APPROACH_ATTACK());
    }
    public bool attack;

    public void Die()
    {
        BodyDeath.GameObject.SetParent(Scene);
        BodyDeath.Enabled = true;
        GameObject.Destroy();
        if(screamSound!=null) screamSound.Stop(0.1f);
    }
    SoundHandle screamSound;
    float lastScream = -1000;
    float lastTryScream = -1000;
    public async void Scream()
    {
        if(Time.Now - lastScream < ScreamTime && Time.Now - lastTryScream < ScreamTime) return;
        lastTryScream = Time.Now;

        var hit = Scene.Trace.Ray(HeadBones.Transform.Position, player.Camera.Transform.Position).IgnoreGameObjectHierarchy(GameObject).WithAnyTags("world","player").UseHitboxes().Run();
        if(hit.GameObject != player.GameObject) return;

        lastScream = Time.Now;
        screamSound = Sound.Play(ScreamSound,  HeadBones.Transform.Position);
        await Task.DelaySeconds(hit.Distance/200);

        hit = Scene.Trace.Ray(HeadBones.Transform.Position, player.Camera.Transform.Position).IgnoreGameObjectHierarchy(GameObject).WithAnyTags("world","player").UseHitboxes().Run();
        if(hit.GameObject != player.GameObject) return;

        
        player.Stunned = 0.5f;
        
    }
    protected override void Update()
    {
        if(healthComponent.Health <= 0)
        {
            Die();
            return;
        }
        if(FindChooseEnemy.Enemy.IsValid())
        {
            stateMachine.ChangeState(FindCondition());
            //FaceThing(FindChooseEnemy.Enemy);
        }
        else
        {
            stateMachine.ChangeState("IDLE");
        }
        if(attack && !isAttacking)
        {
            Attack();
        }
        
        Body.Set("Walking", Agent.Velocity.Length > 1);

        attack = false;

        
    }
    bool isAttacking;
    async void Attack()
    {
        
        isAttacking = true;
        Body.Set("Attack", true);
        await Task.DelaySeconds(AttackTime);
        Body.Set("Attack", false);
        await Task.Frame();
        await Task.Frame();
        isAttacking = false;
    }
    public void FaceThing(GameObject thing)
    {
        Angles angles = Rotation.LookAt(thing.Transform.Position - Transform.Position);
        angles.pitch = 0;
        angles.roll = 0;
        Transform.Rotation = Rotation.Lerp(Transform.Rotation, angles, RotateSpeed * Time.Delta);
    }

    string FindCondition()
    {
        if (FindChooseEnemy.Enemy != null)
        {
            return "APPROACH_ATTACK";
        }
        else
        {
            return "IDLE";
        }
    }

}

public class IDLE : AIState
{
    ScreamerAI screamerAI;
	public void Enter( AIAgent agent )
	{
		screamerAI = agent.Components.Get<ScreamerAI>();
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

	public void Update( AIAgent agent )
	{
		float chance = Time.Delta / screamerAI.RandomMoveTime;
        if(Game.Random.Next(0,100000)/100000f < chance)
        {
            float distanceMod = Game.Random.Next(0,100)/100;
            float distance = (distanceMod * (screamerAI.RandomMoveDis.y-screamerAI.RandomMoveDis.x))+screamerAI.RandomMoveDis.x;
            agent.Agent.MoveTo(agent.Transform.Position+(Vector3.Random.WithZ(0)*distance));
        }
	}
}

public class APPROACH_ATTACK : AIState
{
    ScreamerAI screamerAI;
    float lastDis;
	public void Enter( AIAgent agent )
	{
        lastDis = 100000;
		screamerAI = agent.Components.Get<ScreamerAI>();
        agent.GameObject.SetParent(agent.chunkDealer.ActiveChunk);
	}

	public void Exit( AIAgent agent )
	{
		
	}

	public string GetID()
	{
		return "APPROACH_ATTACK";
	}
	public void Update( AIAgent agent )
	{
        if(screamerAI.FindChooseEnemy.TimeSinceSeen > screamerAI.LooseTime)
        {
            screamerAI.FindChooseEnemy.Enemy = null;
            agent.stateMachine.ChangeState("IDLE");
            return;
        }

        float distance = Vector3.DistanceBetween(agent.Transform.Position,screamerAI.FindChooseEnemy.Enemy.Transform.Position);
        
        if(distance <= screamerAI.ScreamDistance)
        {
            screamerAI.Scream();
        }
        
		agent.Agent.MoveTo(distance > screamerAI.StopDistance ? screamerAI.FindChooseEnemy.Enemy.Transform.Position : agent.Transform.Position);
        screamerAI.attack = distance < screamerAI.AttackDistance;
        if(screamerAI.attack && distance <= screamerAI.StopDistance)
        {
            screamerAI.FaceThing(screamerAI.FindChooseEnemy.Enemy);
        }
        lastDis = distance;
	}
}