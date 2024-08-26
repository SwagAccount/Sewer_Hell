using Sandbox;
using Sandbox.Navigation;

namespace trollface;
[EditorHandle("materials/gizmos/enemyspawner.png")]
public sealed class EnemySpawner : Spawner
{
	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Red;
		base.DrawGizmos();
	}
	public override void SpawnThing(GameObject thing, Vector3 position, bool test = false)
	{
		base.SpawnThing( thing, position, test );
		var spawnPos = Scene.NavMesh.GetClosestPoint(position).Value;
		GameObject spawned = thing.Clone();
		spawned.Transform.Position = spawnPos;
		spawned.Transform.Rotation = spawned.Transform.Rotation.RotateAroundAxis(Vector3.Up, Game.Random.Next(0,360));
		chunkDealer.PlaceInChunk(spawned, test);

	}

}
