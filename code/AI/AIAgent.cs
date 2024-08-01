using Sandbox;
namespace trollface;
public abstract class AIAgent : Component
{
    [Property] public AIStateMachine stateMachine { get; set; }
    public string initialState { get; set; }
    public NavMeshCharacter Controller { get; set; }
    public NavMeshAgent Agent { get; set; }

    protected override void OnStart()
    {
        Controller = Components.GetOrCreate<NavMeshCharacter>();
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
