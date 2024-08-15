using System;
using Editor;
using Sandbox;
namespace trollface;
public abstract class AIAgent : Component
{
    [Hide, Property] public AIStateMachine stateMachine { get; set; }
    [Hide, Property] public string initialState { get; set; }
    [Hide, Property] public NavMeshCharacter Controller { get; set; }
    [Hide, Property] public NavMeshAgent Agent { get; set; }

    protected override void OnStart()
    {
        Controller = Components.GetOrCreate<NavMeshCharacter>();
        Agent = Components.GetOrCreate<NavMeshAgent>();
        Controller.currentTarget = Transform.Position;
        stateMachine = new AIStateMachine(this);

        SetStates();
        InitializeState();
    }

    protected virtual void SetStates()
    {
        
    }

    async void InitializeState()
    {
        await Task.Frame();
        stateMachine.ChangeState(initialState, false);
    }

    protected override void OnUpdate()
    {
        Update();
        stateMachine.Update();
        
    }

    protected virtual void Update()
    {
        
    }
}
