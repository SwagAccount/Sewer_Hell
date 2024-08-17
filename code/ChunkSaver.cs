using System.Text.Json.Nodes;
using Sandbox;

public sealed class ChunkSaver : Component
{
	public void Save()
	{
		if(!FileSystem.Data.DirectoryExists($"Saves/Slot1/{Scene.Name}/Chunks")) FileSystem.Data.CreateDirectory($"Saves/Slot1/{Scene.Name}/Chunks");

		foreach(GameObject c in GameObject.Children)
		{
			if(c.IsPrefabInstance) c.BreakFromPrefab();
		}

		JsonObject SaveData = GameObject.Serialize();
		
		FileSystem.Data.WriteAllText($"Saves/Slot1/{Scene.Name}/Chunks/{GameObject.Name}.json", SaveData.ToJsonString());
	}

	public void Load()
	{
		if(!FileSystem.Data.FileExists($"Saves/Slot1/{Scene.Name}/{GameObject.Name}.json")) return;
		
		GameObject.Deserialize(
			Json.Deserialize<JsonObject>(FileSystem.Data.ReadAllText($"Saves/Slot1/{Scene.Name}/{GameObject.Name}.json"))
		);
	}
}
