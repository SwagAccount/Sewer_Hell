using Sandbox;
using Sandbox.Navigation;
using System;
namespace trollface;
public sealed class ScreamerAI : AIAgent
{
    [Property] public float RotateSpeed {get;set;} = 10f;
    public FindChooseEnemy FindChooseEnemy;
    protected override void SetStates()
    {
        FindChooseEnemy = Components.Get<FindChooseEnemy>();
        initialState = "IDLE";
        stateMachine.RegisterState(new IDLE());
        stateMachine.RegisterState(new APPROACH_ATTACK());
    }

    protected override void Update()
    {
        if(FindChooseEnemy.Enemy.IsValid())
        {
            stateMachine.ChangeState(FindCondition());
            FaceThing(FindChooseEnemy.Enemy);
        }
        else
        {
            stateMachine.ChangeState("IDLE");
        }
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
		agent.Controller.currentTarget = screamerAI.FindChooseEnemy.Enemy.Transform.Position;
	}
}