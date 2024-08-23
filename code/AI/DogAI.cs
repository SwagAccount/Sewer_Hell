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
	[Property] public float JumpWarmTime {get;set;} = 0.625f;
	[Property] public float RunAttackDis {get;set;}
	[Property] public float walkSpeed {get;set;} = 60f;
	[Property] public float runSpeed {get;set;} = 120f;
	[Property] public float RandomMoveTime {get;set;} = 10f;
    [Property] public Vector2 RandomMoveDis {get;set;} = new Vector2(50,100);

    public FindChooseEnemy FindChooseEnemy;
    HealthComponent healthComponent;
	
	public DogAnimState dogAnimState;

	public enum DogAnimState
	{
		IDLE,
		WALK,
		RUN,
		PREPJUMP,
		AIR
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

	Vector3 JumpFacePoint;
	void JumpLogic()
	{
		Body.Set("State", dogAnimState.AsInt());
		if(Agent.Enabled)
		{
			FacePoint(JumpFacePoint);
			Agent.MoveTo(Agent.Transform.Position);
			return;
		}

		if(Rigidbody.Velocity.Length > 10f || Time.Now-jumptime < 0.25f) return;
		jumped = false;
		dogAnimState = DogAnimState.IDLE;
		Agent.Enabled = true;
		Rigidbody.Enabled = false;
	}

	public async void Jump(Vector3 at)
	{
		jumped = true;
		dogAnimState = DogAnimState.PREPJUMP;
		JumpFacePoint = at;
		await Task.DelaySeconds(JumpWarmTime);
		jumptime = Time.Now;
		dogAnimState = DogAnimState.AIR;
		Agent.Enabled = false;
		Rigidbody.Enabled = true;
		Rigidbody.ApplyForce(((at-Transform.Position).Normal+Vector3.Up/3)*JumpForce);
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
		agent.Agent.MoveTo(agent.Transform.Position);
		agent.chunkDealer.PlaceInChunk(agent.GameObject);
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
		agent.Agent.MaxSpeed = dogAI.walkSpeed;
		float chance = Time.Delta / dogAI.RandomMoveTime;
        //Log.Info(chance);   
        if(Game.Random.Next(0,100000)/100000f < chance)
        {
            float distanceMod = Game.Random.Next(0,100)/100;
            float distance = (distanceMod * (dogAI.RandomMoveDis.y-dogAI.RandomMoveDis.x))+dogAI.RandomMoveDis.x;
            agent.Agent.MoveTo(agent.Transform.Position+(Vector3.Random.WithZ(0)*distance));
        }

		dogAI.dogAnimState = agent.Agent.Velocity.Length > dogAI.walkSpeed/2 ? DogAI.DogAnimState.WALK : DogAI.DogAnimState.IDLE;
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
		agent.GameObject.SetParent(agent.chunkDealer.ActiveChunk);
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

		agent.Agent.MaxSpeed = dogAI.runSpeed;
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