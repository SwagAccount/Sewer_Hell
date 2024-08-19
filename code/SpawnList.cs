using System;
namespace trollface;

[GameResource("Spawn List", "sl","List of Prefabs and There Spawn Chances", Icon = "List")]
public sealed class SpawnList : GameResource
{
    [Property] public List<SpawnItem> SpawnItems {get; set;}
    public class SpawnItem
    {
        [KeyProperty] public GameObject Prefab {get;set;}
        [KeyProperty] public int Weight {get;set;} = 1;
        [KeyProperty] public string DisplayedChance {get;set;}
    }

	protected override void PostReload()
	{
		int totalWeight = SpawnItems.Sum(item => item.Weight);
        Log.Info(totalWeight);
        for(int i = 0; i < SpawnItems.Count; i++)
        {
            SpawnItems[i].DisplayedChance = $"{MathF.Round((float)SpawnItems[i].Weight/(float)totalWeight*100f)}%";
        }
	}

	public GameObject GetItem()
    {
        if (SpawnItems == null || SpawnItems.Count == 0)
            return null;

        int totalWeight = SpawnItems.Sum(item => item.Weight);

        int randomWeight = (int)MathF.Round(totalWeight * (Game.Random.Next(0,1000)/1000f));
        int cumulativeWeight = 0;
        foreach (var item in SpawnItems)
        {
            int beforeWeight = cumulativeWeight;
            cumulativeWeight += item.Weight;
            if(randomWeight > beforeWeight && randomWeight <= cumulativeWeight) return item.Prefab;
        }

        return null;
    }
}