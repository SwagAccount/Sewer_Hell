using System;

namespace trollface;

[GameResource("Spawn List", "sl","List of Prefabs and There Spawn Chances", Icon = "List")]
public sealed class SpawnList : GameResource
{
    [Property] public List<SpawnItem> SpawnItems {get; set;}
    public class SpawnItem
    {
        [KeyProperty] public GameObject Prefab {get;set;}
        [KeyProperty] public float Weight {get;set;} = 1;
    }

    public GameObject GetItem()
    {
        if (SpawnItems == null || SpawnItems.Count == 0)
            return null;

        float totalWeight = SpawnItems.Sum(item => item.Weight);

        float randomWeight = totalWeight * (Game.Random.Next(0,1000)/1000f);
        float closestDistance = 1000;
        float cumulativeWeight = 0;
        SpawnItem closestItem = null;
        foreach (var item in SpawnItems)
        {
            cumulativeWeight += item.Weight;
            float dis = MathF.Abs(randomWeight - cumulativeWeight);
            if(dis < closestDistance)
            {
                closestDistance = dis;
                closestItem = item;
            }
        }

        return closestItem.Prefab;
    }
}