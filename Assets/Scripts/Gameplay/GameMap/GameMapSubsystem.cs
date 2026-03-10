using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using InventorySystem;
using System.Data;
using UnityEngine.Tilemaps;

public class GameMapSubsystem : Singleton<GameMapSubsystem>
{
    // Scene
    [Header("Initial Scene Settings")]
    [SerializeField] private bool loadInitialScene = true;
    [SceneName] [SerializeField] private string initialSceneName;

    // Tile visual
    [Header("Tile Visual")]
    public RuleTile dugTile;
    public RuleTile wateredTile;
    
    // Tile map ref
    private Tilemap dugTileMap;
    private Tilemap wateredTileMap;

    public Action<string> onNewSceneLoaded;
    public Action<string> onOldSceneStartUnloading;

    // Persistent Game Map Data
    [Header("Persistent Game Map Data")]
    [SerializeField] private List<PersistentGameMapData_SO> persistentGameMapDataList = new List<PersistentGameMapData_SO>();
    public List<PersistentGameMapData_SO> PersistentGameMapDataList => persistentGameMapDataList;




    // Game Map Save Data for current save file
    private List<GameMapSaveData> gameMapSaveDataList = new List<GameMapSaveData>();

    // Key: sceneName, Value: GameMapSaveData
    private Dictionary<string, GameMapSaveData> gameMapSaveDataDict = new Dictionary<string, GameMapSaveData>();

    public Dictionary<Vector2Int, TileInfo> currentTileInfoDict { get ; private set; } = new Dictionary<Vector2Int, TileInfo>();
    public GameMapSaveData currentGameMapSaveData { get; private set; }

    [Header("Current Scene Info")]
    [SerializeField] private string currentSceneName;

    public Grid currentGrid { get; private set; }

    [Header("Placable Item Data")]
    public PlacableItemDataList_SO placableItemDataList;

    public Dictionary<int, GameObject> placableItemDataDict = new Dictionary<int, GameObject>();

    [Header(" Dropped Item Prefab")]
    public GameObject droppedItemPrefab;
    private int droppedItemPoolIndex = -1; // Index of the dropped item prefab in the ObjectPoolManager's pool list, will be set in GameInstance when initializing the GameMapSubsystem
    private List<DropppedItem> droppedItemsInCurrentScene = new List<DropppedItem>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize(int droppedItemPoolIndex, List<GameMapSaveData> gameMapSaveDataList)
    {
        this.droppedItemPoolIndex = droppedItemPoolIndex;
        this.gameMapSaveDataList = gameMapSaveDataList;

        InitializeGameMapDataDict();
        InitializePlacablePrefabData();

        // TODO: 
        if(!string.IsNullOrEmpty(initialSceneName) && loadInitialScene)
        {
            StartCoroutine(SwitchScene(initialSceneName));
        }
    }

    // Scene Management
    public IEnumerator SwitchScene(string newSceneName)
    {
        // Unload current scene if exists, then load new scene
        if(currentSceneName != null)
        {
            yield return UnloadScene(currentSceneName);
        }

        yield return LoadScene(newSceneName);
        currentSceneName = newSceneName;
    
        // Update currentGameMapSaveData based on new scene name
        if(gameMapSaveDataDict.TryGetValue(newSceneName, out GameMapSaveData data))
        {
            currentGameMapSaveData = data;
        }
        else
        {
            Debug.LogWarning($"No GameMapData found for scene {newSceneName}.");
            currentGameMapSaveData = null;
        }
    }

    public IEnumerator TeleportPlayerToScene(string targetSceneName, GameObject player, Vector2 spawnPosition)
    {
        yield return SwitchScene(targetSceneName);

        // Wait for the fade out and fade in to complete before moving the player, to avoid the player being visible at the original position or the new position during the transition.
        yield return new WaitForSeconds(GameInstance.Instance.gameSettings.transitionFadeDuration);
        player.transform.position = spawnPosition;
    }


    private IEnumerator LoadScene(string sceneName)
    {
        if(sceneName != initialSceneName)
        {
            yield return new WaitForSeconds(GameInstance.Instance.gameSettings.transitionFadeDuration);
        }

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newScene);

        currentGrid = FindObjectOfType<Grid>();

        dugTileMap =     GameObject.FindWithTag("Dug").GetComponent<Tilemap>();
        wateredTileMap = GameObject.FindWithTag("Watered").GetComponent<Tilemap>();

        if(currentGrid == null)
        {
            Debug.LogError($"No Grid found in scene {sceneName}.");
        }
        if(dugTileMap == null)
        {
            Debug.LogError($"No Tilemap with tag 'Dug' found in scene {sceneName}.");
        }
        if(wateredTileMap == null)
        {
            Debug.LogError($"No Tilemap with tag 'Watered' found in scene {sceneName}.");
        }

        currentGameMapSaveData = gameMapSaveDataDict.ContainsKey(sceneName) ? gameMapSaveDataDict[sceneName] : null;
        if(currentGameMapSaveData == null)
        {
            Debug.LogError($"No GameMapData found for scene {sceneName}.");
        }

        LoadGameMapData();

        InitializeTileVisuals();

        onNewSceneLoaded?.Invoke(sceneName);

        Debug.Log("Loaded scene: " + sceneName);
    }

    private IEnumerator UnloadScene(string sceneName)
    {
        if(!String.IsNullOrEmpty(sceneName))
        {
            onOldSceneStartUnloading?.Invoke(sceneName);

            SaveGameMapData();

            yield return new WaitForSeconds(GameInstance.Instance.gameSettings.transitionFadeDuration);

            yield return SceneManager.UnloadSceneAsync(sceneName);
            currentGameMapSaveData = null;

            Debug.Log("Unloaded scene: " + sceneName);
        }
        else
        {
            Debug.LogWarning("Tried to unload a scene with null or empty name.");
        }
        
    }

    void InitializePlacablePrefabData()
    {
        foreach(var data in placableItemDataList.placableItemDataList)
        {
            if(data != null && data.prefab != null)
            {
                if(!placableItemDataDict.ContainsKey(data.itemID))
                {
                    placableItemDataDict.Add(data.itemID, data.prefab);
                }
            }
        }
    }

    // Game Map Data Management
    void InitializeGameMapDataDict()
    {
        gameMapSaveDataDict.Clear();
        foreach(GameMapSaveData data in gameMapSaveDataList)
        {
            if(data != null && !string.IsNullOrEmpty(data.SceneName))
            {
                if(!gameMapSaveDataDict.ContainsKey(data.SceneName))
                {
                    gameMapSaveDataDict.Add(data.SceneName, data);
                }
                else
                {
                    Debug.LogWarning($"Duplicate scene name {data.SceneName} in GameMapDataList.");
                }
            }
        }
    }

    void InitializeTileVisuals()
    {
        if(currentGameMapSaveData == null)
        {
            Debug.LogError($"Current GameMapData is null for scene {currentSceneName}. Cannot initialize tile visuals.");
            return;
        }

        foreach(TileInfo tileInfo in currentGameMapSaveData.tileInfoList)
        {
            if(tileInfo.daySinceDug >= 0)
            {
                dugTileMap.SetTile((Vector3Int)tileInfo.position, dugTile);
            }
            if(tileInfo.isWatered)
            {
                wateredTileMap.SetTile((Vector3Int)tileInfo.position, wateredTile);
            }
        }
    }

    public TileInfo GetTileInfoByGridPos(Vector2Int position)
    {
        if(currentGameMapSaveData == null)
        {
            Debug.LogError($"Current GameMapData is null for scene {currentSceneName}.");
            return null;
        }

        position = new Vector2Int(position.x, position.y);
        if(currentTileInfoDict.ContainsKey(position))
        {
            return currentTileInfoDict[position];
        }
        else
        {
            Debug.LogWarning($"No tile info found for position {position} in scene {currentSceneName}.");
            return null;
        }
    }

    public TileInfo GetTileInfoByWorldPos(Vector3 position)
    {
        if(currentGrid == null)
        {
            Debug.LogError($"Current Grid is null in scene {currentSceneName}. Cannot get tile info by world position.");
            return null;
        }
        Vector3Int cellPos = currentGrid.WorldToCell(position);
        return GetTileInfoByGridPos((Vector2Int)cellPos);
    }

    public Vector3 GetWorldPositionByTileInfo(TileInfo tileInfo)
    {
        if(currentGrid == null)
        {
            Debug.LogError($"Current Grid is null in scene {currentSceneName}. Cannot get world position by tile info.");
            return Vector3.zero;
        }
        return currentGrid.GetCellCenterWorld((Vector3Int)tileInfo.position);
    } 

    public void UpdateDugTile(TileInfo tile, bool isDug, int deltaDay)
    {
        Vector3Int cellPos = (Vector3Int)tile.position;
        if(isDug)
        {
            tile.daySinceDug += deltaDay;
            dugTileMap.SetTile(cellPos, dugTile);
        }
        else
        {
            tile.daySinceDug = -1;
            dugTileMap.SetTile(cellPos, null);
        }
    }

    public void UpdateWateredTile(TileInfo tile, bool isWatered)
    {
        Vector3Int cellPos = (Vector3Int)tile.position;
        tile.isWatered = isWatered;
        if(isWatered)
        {
            wateredTileMap.SetTile(cellPos, wateredTile);
        }
        else
        {
            wateredTileMap.SetTile(cellPos, null);
        }
    }

    public GameObject GetPlacablePrefab(int itemID)
    {
        return placableItemDataDict.ContainsKey(itemID) ? placableItemDataDict[itemID] : null;
    }
    public void PlaceFurniture(TileInfo tile, ItemDefinition itemDef)
    {
        tile.isOccupied = true;
        Vector3 worldPos = currentGrid.GetCellCenterWorld((Vector3Int)tile.position);
        GameObject furniturePrefab = GetPlacablePrefab(itemDef.itemID);
        GameInstance.Instance.SpawnGameObjectInWorld(furniturePrefab, worldPos, Quaternion.identity);
    }

    public void PlaceThing(TileInfo tile)
    {
        tile.isOccupied = true;
    }


    // Game Map Data Persistence
    private void SaveGameMapData()
    {
        if(currentGameMapSaveData == null)
        {
            Debug.LogError($"Current GameMapData is null for scene {currentSceneName}. Cannot save.");
            return;
        }

        SaveAllDroppedItems();
        SaveAllResources();
    }

    public void RegisterDroppedItem(DropppedItem item)
    {
        if(!droppedItemsInCurrentScene.Contains(item))
        {
            droppedItemsInCurrentScene.Add(item);
        }
    }

    public void UnregisterDroppedItem(DropppedItem item)
    {
        if(droppedItemsInCurrentScene.Contains(item))
        {
            droppedItemsInCurrentScene.Remove(item);
        }
    }

    private void SaveAllDroppedItems()
    {
        // Clear existing dropped items data
        currentGameMapSaveData.droppedItems.Clear();

        // We cant use foreach loop here because we will be releasing the dropped item to pool in the loop, which will modify the droppedItemsInCurrentScene list and cause issues with the foreach loop. So we will use for loop instead and always access the first element in the list until the list is empty.
        for(int i = 0; i < droppedItemsInCurrentScene.Count; i++)
        {
            DropppedItem item = droppedItemsInCurrentScene[i];
            if(item != null)
            {
                DroppedItemSaveData itemData = new DroppedItemSaveData(item.itemID, item.itemCount, item.transform.position);
                currentGameMapSaveData.droppedItems.Add(itemData);
                ReleaseDroppedItemToPool(item);
            }
        }

        // Unregister the item from the list regardless of whether it's null or not, to ensure that the list is cleared for the next time we load this scene. 
        // This is important because when we load a scene, we will spawn new dropped items based on the saved data, and we don't want old dropped items from previous play sessions to be mixed in.
        droppedItemsInCurrentScene.Clear();
    }

    private void SaveAllResources()
    {
        ResourceSubsystem.Instance.SaveAllResources(currentGameMapSaveData.resources);
    }

    private void LoadGameMapData()
    {
        if(currentGameMapSaveData == null)
        {
            Debug.LogError($"Current GameMapData is null for scene {currentSceneName}. Cannot read.");
            return;
        }
        currentTileInfoDict.Clear();
        foreach(TileInfo tileInfo in currentGameMapSaveData.tileInfoList)
        {
            currentTileInfoDict[tileInfo.position] = tileInfo;
        }
        
        InitializeTileVisuals();

        LoadAllDroppedItems();
        LoadAllResources();
    }

    private void LoadAllDroppedItems()
    {
        foreach(DroppedItemSaveData itemData in currentGameMapSaveData.droppedItems)
        {
            RegisterDroppedItem(SpawnDroppedItemInWorld(itemData.itemID, itemData.itemCount, itemData.position, DroppedItemSource.FromSaveData));
        }
    }

    public DropppedItem SpawnDroppedItemInWorld(int itemID, int count, Vector3 spawnPosition, DroppedItemSource source)
    {
        ItemDefinition itemDef = InventorySubsystem.Instance.GetItemDefinition(itemID);
        if(itemDef != null)
        {
            GameObject droppedItem = ObjectPoolManager.Instance.GetObjectFromPool(droppedItemPoolIndex);

            DropppedItem droppedItemComp = droppedItem.GetComponent<DropppedItem>();

            droppedItemComp.Initialize(itemID, count);
            droppedItem.transform.position = spawnPosition;

            // Only play dropping animation when the item is dropping from the world or a inventory.
            switch(source)
            {
               case DroppedItemSource.FromSaveData:
               case DroppedItemSource.FromGameDesign:
                    break;
                default:
                    droppedItemComp.PlayDroppingAnim();
                    break;
            }

            droppedItemComp.ResetPickupCD(source);
            droppedItemComp.PickupCoolingDown();


            return droppedItemComp;
        }
        return null;
    }

    public DropppedItem SpawnDroppedItemInWorld(ItemInstance itemInstance, Vector3 spawnPosition, DroppedItemSource source)
    {
        if(itemInstance == null || itemInstance.ItemDefinition == null || !itemInstance.ItemDefinition.IsValidItem())
        {
            Debug.LogWarning("Tried to spawn invalid item.");
            return null;
        }
        int itemID = itemInstance.ItemDefinition.itemID;
        int count = itemInstance.stackCount;
        return SpawnDroppedItemInWorld(itemID, count, spawnPosition, source);
    }

    public void ReleaseDroppedItemToPool(DropppedItem item)
    {
        ObjectPoolManager.Instance.ReleaseObjectToPool(droppedItemPoolIndex, item.gameObject);
    }

    private void LoadAllResources()
    {
        ResourceSubsystem.Instance.LoadAllResources(currentGameMapSaveData.resources);
    }
}
