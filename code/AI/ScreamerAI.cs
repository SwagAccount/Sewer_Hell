using Sandbox;
using Sandbox.Navigation;
using System;
using System.Threading.Tasks;
namespace trollface;
public sealed class ScreamerAI : AIAgent
{
    [Property] public SkinnedModelRenderer Body {get; set;}
    [Property] public BodyDeath BodyDeath {get; set;}
    [Property] public float RotateSpeed {get;set;} = 10f;
    [Property] public float AttackDistance {get;set;} = 30f;
    [Property] public float StopDistance {get;set;} = 10f;
    [Property] public float AttackTime {get;set;} = 0.28f;
    public FindChooseEnemy FindChooseEnemy;
    HealthComponent healthComponent;
    protected override void SetStates()
    {
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
            FaceThing(FindChooseEnemy.Enemy);
        }
        else
        {
            stateMachine.ChangeState("IDLE");
        }
        
        if(attack && !isAttacking)
        {
            Attack();
        }
        
        Body.Set("Walking", Controller.velocity.Length > 1);

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
		agent.Controller.currentTarget = agent.Transform.Position;
	}
}

public class APPROACH_ATTACK : AIState
{
    ScreamerAI screamerAI;
	public void Enter( AIAgent agent )
	{
		screamerAI = agent.Components.Get<ScreamerAI>();
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
        float distance = Vector3.DistanceBetween(agent.Transform.Position,screamerAI.FindChooseEnemy.Enemy.Transform.Position);
		agent.Controller.currentTarget = distance > screamerAI.StopDistance ? screamerAI.FindChooseEnemy.Enemy.Transform.Position : agent.Transform.Position;
        screamerAI.attack = distance < screamerAI.AttackDistance;
	}
}