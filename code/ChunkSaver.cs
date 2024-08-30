using System.Text.Json.Nodes;
using Sandbox;

public sealed class ChunkSaver : Component
{
	public void Save(string saveFolder, int slot, string levelName)
	{
		if(!FileSystem.Data.DirectoryExists($"{saveFolder}/Slot{slot}/{levelName}/Chunks")) FileSystem.Data.CreateDirectory($"{saveFolder}/Slot{slot}/{levelName}/Chunks");

		foreach(GameObject c in GameObject.Children)
		{
			if(c.IsPrefabInstance) c.BreakFromPrefab();
		}

		JsonObject SaveData = GameObject.Serialize();
		SceneUtility.MakeIdGuidsUnique(SaveData);
		FileSystem.Data.WriteAllText($"{saveFolder}/Slot{slot}/{levelName}/Chunks/{GameObject.Name}.json", SaveData.ToJsonString());
	}

	public void Load(string saveFolder, int slot, string levelName)
	{
		if(!FileSystem.Data.FileExists($"{saveFolder}/Slot{slot}/{levelName}/Chunks/{GameObject.Name}.json")) return;

		foreach(GameObject c in GameObject.Children)
		{
			c.Destroy();
		}
		Components.Get<ChunkSaver>();
		JsonObject jObject = Json.Deserialize<JsonObject>(FileSystem.Data.ReadAllText($"{saveFolder}/Slot{slot}/{levelName}/Chunks/{GameObject.Name}.json"));
		jObject.Remove("Component");
		GameObject.Deserialize(
			jObject
		);
	}
}
