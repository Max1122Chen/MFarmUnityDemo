using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

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

public class ContainerSaveData
{
    public int containerID;
    public Vector2 position;
    public int inventorySize;
    public List<ItemInstance> items;

    public ContainerSaveData(int id, Vector2 pos, int inventorySize, List<ItemInstance> itemList)
    {
        containerID = id;
        position = pos;
        this.inventorySize = inventorySize;
        items = itemList;
    }
}

[System.Serializable]
public class GameMapSaveData
{
    [SerializeField] private string sceneName;
    public string SceneName => sceneName;
    public List<TileInfo> tileInfoList = new List<TileInfo>();
    public Vector2Int mapSize;
    public Vector2Int lowerLeftTileOriginalPos;
    public List<DroppedItemSaveData> droppedItems = new List<DroppedItemSaveData>();
    public List<ResourceSaveData> resources = new List<ResourceSaveData>();
    public List<ContainerSaveData> containers = new List<ContainerSaveData>();

    private GameMapSaveData() {} // Private constructor to prevent direct instantiation, we will use the constructor with PersistentGameMapData_SO parameter to create a new instance of GameMapSaveData.
    public GameMapSaveData(PersistentGameMapData_SO persistentGameMapData)
    {
        sceneName = persistentGameMapData.sceneName;

        tileInfoList = new List<TileInfo>(persistentGameMapData.tileInfoList.Count);
        mapSize = persistentGameMapData.mapSize;
        lowerLeftTileOriginalPos = persistentGameMapData.lowerLeftTileOriginalPos;

        // Copy the persisten tile info data to the game map save data, we will update the tile info data when we save the game, so we need to copy the data here to avoid reference issue.
        for (int i = 0; i < persistentGameMapData.tileInfoList.Count; i++)
        {
            tileInfoList.Add(new TileInfo(persistentGameMapData.tileInfoList[i]));
        }
    }
}

[System.Serializable]
public class GameSaveData
{
    public int saveIndex;
    public string playerName;

    // public PlayerSaveData playerSaveData; // we will implement this later when we have the player data to save
    public List<GameMapSaveData> gameMapSaveDataList = new List<GameMapSaveData>();



}
