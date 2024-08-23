using System.Text.Json.Nodes;
using Sandbox;

public sealed class ChunkSaver : Component
{
	public void Save(int slot)
	{
		if(!FileSystem.Data.DirectoryExists($"Saves/Slot{slot}/{Scene.Name}/Chunks")) FileSystem.Data.CreateDirectory($"Saves/Slot{slot}/{Scene.Name}/Chunks");

		foreach(GameObject c in GameObject.Children)
		{
			if(c.IsPrefabInstance) c.BreakFromPrefab();
		}

		JsonObject SaveData = GameObject.Serialize();
		SceneUtility.MakeIdGuidsUnique(SaveData);
		FileSystem.Data.WriteAllText($"Saves/Slot{slot}/{Scene.Name}/Chunks/{GameObject.Name}.json", SaveData.ToJsonString());
	}

	public void Load(int slot)
	{
		if(!FileSystem.Data.FileExists($"Saves/Slot{slot}/{Scene.Name}/Chunks/{GameObject.Name}.json")) return;

		foreach(GameObject c in GameObject.Children)
		{
			c.Destroy();
		}
		
		GameObject.Deserialize(
			Json.Deserialize<JsonObject>(FileSystem.Data.ReadAllText($"Saves/Slot{slot}/{Scene.Name}/Chunks/{GameObject.Name}.json"))
		);
	}
}
