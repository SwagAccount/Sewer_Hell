namespace trollface;

[EditorHandle("materials/gizmos/spawnpoint.png")]
public sealed class CoverContext : Component
{
    [Property] public float angle {get;set;}
    [Property] public bool owned {get;set;}
    [Property] public bool wall {get;set;}
}