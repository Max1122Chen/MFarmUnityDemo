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
    public Vector2Int mapOffset;
    public List<DroppedItemSaveData> droppedItems = new List<DroppedItemSaveData>();
    public List<ResourceSaveData> resources = new List<ResourceSaveData>();
    public List<ContainerSaveData> containers = new List<ContainerSaveData>();

    private GameMapSaveData() {} // Private constructor to prevent direct instantiation, we will use the constructor with PersistentGameMapData_SO parameter to create a new instance of GameMapSaveData.
    public GameMapSaveData(PersistentGameMapData_SO persistentGameMapData)
    {
        sceneName = persistentGameMapData.sceneName;

        tileInfoList = new List<TileInfo>(persistentGameMapData.tileInfoList.Count);
        mapSize = persistentGameMapData.mapSize;
        mapOffset = persistentGameMapData.mapOffset;

        // Copy the persisten tile info data to the game map save data, we will update the tile info data when we save the game, so we need to copy the data here to avoid reference issue.
        for (int i = 0; i < persistentGameMapData.tileInfoList.Count; i++)
        {
            tileInfoList.Add(new TileInfo(persistentGameMapData.tileInfoList[i]));
        }
    }
}

[System.Serializable]
public class NPCSaveData
{
    public string npcName;  // Also used as the unique identifier for the NPC, we will use this to find the corresponding NPCData in the NPC subsystem when we load the game.
    [SceneName] public string currentScene;
    public Vector2 position;
}

[System.Serializable]
public class InventoryItemSaveData
{
    public int slotIndex = -1;
    public int itemID = -1;
    public int stackCount = 0;
}

[System.Serializable]
public class PlayerSaveData
{
    public string playerName;
    public Vector2 position;
    public int money;
    public List<InventoryItemSaveData> playerInventory = new List<InventoryItemSaveData>();
}



[System.Serializable]
public class VendorSaveData
{
    public int vendorID;    // we will use this to find the corresponding VendorData in the Vendor subsystem when we load the game.
    public List<CommodityInstance> commoditySaveDataList;   // we will use this to update the commodity list of the vendor when we load the game, so we need to save the commodity list here.
}

[System.Serializable]
public class GameSaveData
{
    public int saveIndex;
    public bool firstTimePlaying = true; // we can use this to determine whether it's the player's first time playing the game, and we can show some tutorial tips or something like that based on this flag.
    public string currentScene; // we will use this to determine which scene to load when we load the game, and we will also use this to determine which NPCs to spawn in the world based on the NPC save data.

    public PlayerSaveData playerSaveData; // we will implement this later when we have the player data to save
    public List<GameMapSaveData> gameMapSaveDataList = new List<GameMapSaveData>();
    public List<NPCSaveData> npcSaveDataList = new List<NPCSaveData>();

    public List<VendorSaveData> vendorSaveDataList = new List<VendorSaveData>();

}
