using Sandbox;
using Sandbox.Navigation;
using System.Text.RegularExpressions;

namespace trollface;

[EditorHandle("materials/gizmos/itemspawner.png")]
public sealed class ItemSpawner : Spawner
{
	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Green;
		base.DrawGizmos();
	}
	public override void SpawnThing(GameObject thing, Vector3 position, bool test = false)
	{
		GameObject spawned = thing.Clone();
		spawned.Transform.Position = position;
		spawned.Transform.Rotation = spawned.Transform.Rotation.RotateAroundAxis(Vector3.Up, Game.Random.Next(0,360));
		chunkDealer.PlaceInChunk(spawned, test);
	}
}
