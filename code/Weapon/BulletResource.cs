[GameResource("Bullet", "bullet", "Bullet Data", Icon = "Toggle Off")]
public sealed class Bullet : GameResource
{
    [Property] public GameObject PrefabRef {get;set;}
    [Property] public int Count {get;set;} = 1;
    [Property] public Vector2 Spread {get;set;}
    [Property] public float Grain {get;set;}
    [Property] public float Diameter {get;set;}
    [Property] public float BaseVelocity {get;set;}
}