using Sandbox;
using Sandbox.Navigation;

namespace trollface;

public sealed class EnemySpawner : Spawner
{
	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Red;
		base.DrawGizmos();
	}
	public override void SpawnThing(GameObject thing, Vector3 position, bool test = false)
	{
		var spawnPos = Scene.NavMesh.GetClosestPoint(position);
		GameObject spawned = thing.Clone();
		spawned.Transform.Position = position;
		spawned.Transform.Rotation = spawned.Transform.Rotation.RotateAroundAxis(Vector3.Up, Game.Random.Next(0,360));
		chunkDealer.PlaceInChunk(spawned, test);
		
	}
}
