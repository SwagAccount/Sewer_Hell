using System;

namespace trollface;
public abstract class Spawner : Component
{
    [Property] public SpawnList SpawnList {get;set;}
	[Property] public Vector2 SpawnCount {get;set;}
	[Property] public float SpawnRadius {get;set;}
	[Property] public bool LockZ {get;set;}
    [Property] public bool SpawnTest {get; set;}

    public ChunkDealer chunkDealer;

	protected override void DrawGizmos()
	{
		if(!LockZ) Gizmo.Draw.LineSphere(Vector3.Zero,SpawnRadius);
        else Gizmo.Draw.LineCircle(Vector3.Zero, Vector3.Up,SpawnRadius);
        if(SpawnTest)
        {
            SpawnTest = false;
            Spawn(true);
        }
	}
	protected override void OnStart()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();
	}
	public virtual void Spawn( bool test = false)
    {
        int spawnCount = Game.Random.Next((int)SpawnCount.x,(int)SpawnCount.y+1);
		for (int i = 0; i < spawnCount; i++)
		{
            var pos = Transform.Position + (Vector3.Random * SpawnRadius);
            SpawnThing(SpawnList.GetItem(), LockZ ? pos.WithZ(Transform.Position.z) : pos, test);
		}
    }

    public virtual void SpawnThing(GameObject thing, Vector3 position, bool test = false)
    {

    }
}