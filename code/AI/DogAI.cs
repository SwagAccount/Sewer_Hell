using Sandbox;
using Sandbox.Navigation;
using System;
using System.Threading.Tasks;
namespace trollface;
public sealed class DogAI : AIAgent
{
	[Property] public SkinnedModelRenderer Body {get;set;}
	[Property] public Rigidbody Rigidbody {get;set;}
	[Property] public float RotateSpeed {get;set;}
	[Property] public float JumpDis {get;set;}
	[Property] public float JumpForce {get;set;} = 10560;
	[Property] public float RunAttackDis {get;set;}
	[Property] public float walkSpeed {get;set;}
	[Property] public float runSpeed {get;set;} = 120f;
    public FindChooseEnemy FindChooseEnemy;
    HealthComponent healthComponent;
	
	public DogAnimState dogAnimState;

	public enum DogAnimState
	{
		IDLE,
		WALK,
		RUN
	}
    protected override void SetStates()
    {
        healthComponent = Components.Get<HealthComponent>();
        FindChooseEnemy = Components.Get<FindChooseEnemy>();
        initialState = "DOGIDLE";
        stateMachine.RegisterState(new DOGIDLE());
        stateMachine.RegisterState(new DOGATTACK());
    }
    public void Die()
    {
		Body.GameObject.SetParent(Scene);
		Body.Set("Dead", true);
        GameObject.Destroy();
    }
	[Property] public bool jumped;
    protected override void Update()
    {
        if(healthComponent.Health <= 0)
        {
            Die();
            return;
        }
		if(jumped) { JumpLogic(); return; }

		Body.Set("State", dogAnimState.AsInt());

		//Jump();

        if(FindChooseEnemy.Enemy.IsValid())
        {
            stateMachine.ChangeState(FindCondition());
        }
        else
        {
            stateMachine.ChangeState("DOGIDLE");
			
        }

		//FacePoint(Agent.TargetPosition.HasValue ? Agent.TargetPosition.Value : Transform.Position);
    }
	float jumptime;		
	void JumpLogic()
	{
		if(Rigidbody.Velocity.Length <= 10f && Time.Now-jumptime > 0.25f)
		{
			jumped = false;
			Body.Set("speed", 1);
			Agent.Enabled = true;
			Rigidbody.Enabled = false;
		}
	}

	public void Jump(Vector3 at)
	{
		jumptime = Time.Now;
		Log.Info("Jump");
		Body.Set("speed", 0);
		Agent.Enabled = false;
		Rigidbody.Enabled = true;
		Rigidbody.ApplyForce((at-Transform.Position).Normal*JumpForce);
		jumped = true;
	}

    public void FaceThing(GameObject thing)
    {
        FacePoint(thing.Transform.Position);
    }

	public void FacePoint(Vector3 point)
    {
        Angles angles = Rotation.LookAt(point - Transform.Position);
        angles.pitch = 0;
        angles.roll = 0;
        Transform.Rotation = Rotation.Lerp(Transform.Rotation, angles, RotateSpeed * Time.Delta);
    }

    string FindCondition()
    {
        if (FindChooseEnemy.Enemy != null)
        {
            return "DOGATTACK";
        }
        else
        {
            return "DOGIDLE";
        }
    }

}

public class DOGIDLE : AIState
{
    DogAI dogAI;
	public void Enter( AIAgent agent )
	{
		dogAI = agent.Components.Get<DogAI>();
	}

	public void Exit( AIAgent agent )
	{
		
	}

	public string GetID()
	{
		return "DOGIDLE";
	}

	public void Update( AIAgent agent )
	{
		agent.Agent.MoveTo(agent.Transform.Position);
		dogAI.dogAnimState = DogAI.DogAnimState.IDLE;
	}
}

public class DOGATTACK : AIState
{
    DogAI dogAI;
	bool doJump;
	public void Enter( AIAgent agent )
	{
		dogAI = agent.Components.Get<DogAI>();
		doJump = Vector3.DistanceBetween(agent.Transform.Position,dogAI.FindChooseEnemy.Enemy.Transform.Position) > dogAI.RunAttackDis;
	}

	public void Exit( AIAgent agent )
	{
		
	}
	
	public string GetID()
	{
		return "DOGATTACK";
	}
	public void Update( AIAgent agent )
	{
		
		if(dogAI.jumped) return;
		
		agent.Controller.Speed = dogAI.runSpeed;
		agent.Agent.MoveTo(dogAI.FindChooseEnemy.Enemy.Transform.Position);
		dogAI.dogAnimState = DogAI.DogAnimState.RUN;
		
		float distance = Vector3.DistanceBetween(agent.Transform.Position,dogAI.FindChooseEnemy.Enemy.Transform.Position);
		if(doJump)
		{
			if(distance < dogAI.JumpDis)
			{
				dogAI.Jump(dogAI.FindChooseEnemy.Enemy.Transform.Position+dogAI.FindChooseEnemy.EnemyRelations.attackPoint);
				doJump = false;
			}
		}
		else
		{
			Vector3 dir = (agent.Transform.Position - dogAI.FindChooseEnemy.Enemy.Transform.Position).Normal;
			agent.Agent.MoveTo(dir * 150);
			doJump = Vector3.DistanceBetween(agent.Transform.Position,dogAI.FindChooseEnemy.Enemy.Transform.Position) > dogAI.RunAttackDis;
		}
	}
}