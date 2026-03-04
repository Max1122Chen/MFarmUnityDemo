using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum TileType
{
    None,
    Diggable,
    ItemDroppable,
    FurniturePlaceable,
}

[System.Serializable]
public class TileInfo
{
    public Vector2Int position;

    // Tile Properties
    public bool diggable = false;
    public bool itemDroppable = false;
    public bool furniturePlacable = false;

    // Tile State
    public bool isWatered = false;
    public bool hasThing = false;
    public int daySinceDug = -1;
    public int seedID = -1;

    public override string ToString()
    {
        return $"Pos: {position}, Diggable: {diggable}, ItemDroppable: {itemDroppable}, FurniturePlacable: {furniturePlacable}, DaySinceDug: {daySinceDug}, SeedID: {seedID}, HasThing: {hasThing}";
    }
}

[System.Serializable]
public class DroppedItemSaveData
{
    public int itemID;
    public int itemCount;
    public Vector2 position;

    public DroppedItemSaveData(int id, int count, Vector2 pos)
    {
        itemID = id;
        itemCount = count;
        position = pos;
    }
}

[System.Serializable]
public class ResourceSaveData
{
    public int resourceID;
    public Vector2 position;
    public int growthStageIndex;
    public int growthTimeCounter;

    public ResourceSaveData(int id, Vector2 pos, int stageIndex, int timeCounter)
    {
        resourceID = id;
        position = pos;
        growthStageIndex = stageIndex;
        growthTimeCounter = timeCounter;
    }
}


[System.Serializable]
public class PlacableInfo
{
    
}
//
[CreateAssetMenu(fileName = "GameMapData_SO", menuName = "GameMap/GameMapData_SO")]
public class GameMapData_SO : ScriptableObject
{
    [SceneName] [SerializeField] private string sceneName;

    public string SceneName => sceneName;
    public List<TileInfo> tileInfoList = new List<TileInfo>();

    public List<DroppedItemSaveData> droppedItems = new List<DroppedItemSaveData>();
    public List<ResourceSaveData> resources = new List<ResourceSaveData>();

}
