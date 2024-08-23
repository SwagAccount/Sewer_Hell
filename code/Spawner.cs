using System;
using System.Text.RegularExpressions;

namespace trollface;

[EditorHandle("materials/gizmo/ui.png")]
public abstract class Spawner : Component
{
    [Property] public SpawnList SpawnList {get;set;}
	[Property] public Vector2 SpawnCount {get;set;}
	[Property] public float SpawnRadius {get;set;}
	[Property] public float zOffset {get;set;}
	[Property] public bool LockZ {get;set;}
    [Property] public bool SpawnTest {get; set;}

    public ChunkDealer chunkDealer;
	protected override void DrawGizmos()
	{
		if(!LockZ) Gizmo.Draw.LineSphere(Vector3.Zero,SpawnRadius);
        else Gizmo.Draw.LineCircle(Vector3.Zero, Vector3.Up,SpawnRadius);
        if(zOffset!=0)
        {
            Gizmo.Draw.Arrow(Vector3.Zero.WithZ(zOffset), Vector3.Zero , zOffset/5, zOffset/10);
        }
        if(SpawnTest)
        {
            SpawnTest = false;
            Spawn(true);
        }

        MatchCollection matches = Regex.Matches(SpawnList.ResourceName, @"\d+");
        int largest = int.MinValue;
        foreach (Match match in matches)
        {
            int number = int.Parse(match.Value);
            if (number > largest)
            {
                largest = number;
            }
        }

        Gizmo.Draw.Text((matches.Count > 0 ? largest : 0).ToString(), new Transform(Vector3.Up*10));
	}
	protected override void OnStart()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();
	}
	public virtual void Spawn( bool test = false)
    {
        int spawnCount = Game.Random.Next((int)SpawnCount.x,(int)SpawnCount.y+1);
        if(SpawnCount.y == 1) Log.Info(spawnCount);
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