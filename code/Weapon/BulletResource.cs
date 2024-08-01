[GameResource("Bullet", "bullet", "Bullet Data", Icon = "Toggle Off")]
public sealed class Bullet : GameResource
{
    [Property] public int Count {get;set;} = 1;
    [Property] public float Grain {get;set;}
    [Property] public float Diameter {get;set;}
    [Property] public float BaseVelocity {get;set;}
}