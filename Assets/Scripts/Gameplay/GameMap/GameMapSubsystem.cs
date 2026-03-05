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
    [SceneName] [SerializeField] private string initialSceneName;

    // Tile visualization
    [Header("Tile Visualization")]
    public RuleTile dugTile;
    public RuleTile wateredTile;
    private Tilemap dugTileMap;
    private Tilemap wateredTileMap;

    public Action<string> onNewSceneLoaded;
    public Action<string> onOldSceneStartUnloading;

    // Game Map Data
    [SerializeField] private List<GameMapData_SO> gameMapDataList = new List<GameMapData_SO>();

    // Key: sceneName, Value: GameMapData_SO
    private Dictionary<string, GameMapData_SO> gameMapDataDict = new Dictionary<string, GameMapData_SO>();

    [SerializeField] private GameMapData_SO currentGameMapData;
    public Dictionary<Vector2Int, TileInfo> currentTileInfoDict { get ; private set; } = new Dictionary<Vector2Int, TileInfo>();
    [SerializeField] private string currentSceneName;

    public Grid currentGrid { get; private set; }
    public GameMapData_SO CurrentGameMapData => currentGameMapData;

    [Header("Placable Prefab Data")]
    public PlacablePrefab_SO placablePrefab_SO;

    public Dictionary<int, GameObject> placablePrefabDict = new Dictionary<int, GameObject>();

    [Header(" Dropped Item Prefab")]
    public GameObject droppedItemPrefab;
    private int droppedItemPoolIndex = -1; // Index of the dropped item prefab in the ObjectPoolManager's pool list, will be set in GameInstance when initializing the GameMapSubsystem
    private List<DropppedItem> droppedItemsInCurrentScene = new List<DropppedItem>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize(int droppedItemPoolIndex)
    {
        this.droppedItemPoolIndex = droppedItemPoolIndex;
        InitializeGameMapDataDict();
        InitializePlacablePrefabData();

        // TODO: 
        if(!string.IsNullOrEmpty(initialSceneName))
        {
            StartCoroutine(SwitchScene(initialSceneName));
        }
        else
        {
            Debug.LogWarning("Initial scene name is not set in GameMapSubsystem.");
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
    
        // Update currentGameMapData based on new scene name
        if(gameMapDataDict.TryGetValue(newSceneName, out GameMapData_SO data))
        {
            currentGameMapData = data;
        }
        else
        {
            Debug.LogWarning($"No GameMapData found for scene {newSceneName}.");
            currentGameMapData = null;
        }
    }

    public IEnumerator TeleportPlayerToScene(string targetSceneName, GameObject player, Vector2 spawnPosition)
    {
        yield return SwitchScene(targetSceneName);

        // Wait for the fade out and fade in to complete before moving the player, to avoid the player being visible at the original position or the new position during the transition.
        yield return new WaitForSeconds(GameInstance.Instance.gameSettings.transitionFadeDuration * 2 - 0.2f);
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

        currentGameMapData = gameMapDataDict.ContainsKey(sceneName) ? gameMapDataDict[sceneName] : null;
        if(currentGameMapData == null)
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
            currentGameMapData = null;

            Debug.Log("Unloaded scene: " + sceneName);
        }
        else
        {
            Debug.LogWarning("Tried to unload a scene with null or empty name.");
        }
        
    }

    void InitializePlacablePrefabData()
    {
        foreach(var data in placablePrefab_SO.placablePrefabDataList)
        {
            if(data != null && data.prefab != null)
            {
                if(!placablePrefabDict.ContainsKey(data.itemID))
                {
                    placablePrefabDict.Add(data.itemID, data.prefab);
                }
            }
        }
    }

    // Game Map Data Management
    void InitializeGameMapDataDict()
    {
        gameMapDataDict.Clear();
        foreach(GameMapData_SO data in gameMapDataList)
        {
            if(data != null && !string.IsNullOrEmpty(data.SceneName))
            {
                if(!gameMapDataDict.ContainsKey(data.SceneName))
                {
                    gameMapDataDict.Add(data.SceneName, data);
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
        if(currentGameMapData == null)
        {
            Debug.LogError($"Current GameMapData is null for scene {currentSceneName}. Cannot initialize tile visuals.");
            return;
        }

        foreach(TileInfo tileInfo in currentGameMapData.tileInfoList)
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
        if(currentGameMapData == null)
        {
            Debug.LogError($"Current GameMapData is null for scene {currentSceneName}.");
            return null;
        }

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
            return null;
        }
        Vector3Int cellPos = currentGrid.WorldToCell(position);
        return GetTileInfoByGridPos((Vector2Int)cellPos);
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
        return placablePrefabDict.ContainsKey(itemID) ? placablePrefabDict[itemID] : null;
    }
    public void PlaceFurniture(TileInfo tile, ItemDefinition itemDef)
    {
        tile.hasThing = true;
        Vector3 worldPos = currentGrid.GetCellCenterWorld((Vector3Int)tile.position);
        GameObject furniturePrefab = GetPlacablePrefab(itemDef.itemID);
        GameInstance.Instance.SpawnGameObjectInWorld(furniturePrefab, worldPos, Quaternion.identity);
    }

    public void PlaceThing(TileInfo tile)
    {
        tile.hasThing = true;
    }


    // Game Map Data Persistence
    private void SaveGameMapData()
    {
        if(currentGameMapData == null)
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
        currentGameMapData.droppedItems.Clear();

        foreach(DropppedItem item in droppedItemsInCurrentScene)
        {
            if(item != null)
            {
                DroppedItemSaveData itemData = new DroppedItemSaveData(item.itemID, item.itemCount, item.transform.position);
                currentGameMapData.droppedItems.Add(itemData);
                ReleaseDroppedItemToPool(item);
            }

            // Unregister the item from the list regardless of whether it's null or not, to ensure that the list is cleared for the next time we load this scene. 
            // This is important because when we load a scene, we will spawn new dropped items based on the saved data, and we don't want old dropped items from previous play sessions to be mixed in.
            droppedItemsInCurrentScene.Clear();
        }
    }

    private void SaveAllResources()
    {
        ResourceSubsystem.Instance.SaveAllResources(currentGameMapData.resources);
    }

    private void LoadGameMapData()
    {
        if(currentGameMapData == null)
        {
            Debug.LogError($"Current GameMapData is null for scene {currentSceneName}. Cannot read.");
            return;
        }
        currentTileInfoDict.Clear();
        foreach(TileInfo tileInfo in currentGameMapData.tileInfoList)
        {
            currentTileInfoDict[tileInfo.position] = tileInfo;
        }
        
        InitializeTileVisuals();

        LoadAllDroppedItems();
        LoadAllResources();
    }

    private void LoadAllDroppedItems()
    {
        foreach(DroppedItemSaveData itemData in currentGameMapData.droppedItems)
        {
            RegisterDroppedItem(SpawnDroppedItemInWorld(itemData.itemID, itemData.itemCount, itemData.position));
        }
    }

    public DropppedItem SpawnDroppedItemInWorld(int itemID, int count, Vector3 spawnPosition)
    {
        ItemDefinition itemDef = InventorySubsystem.Instance.GetItemDefinition(itemID);
        if(itemDef != null)
        {
            GameObject droppedItem = ObjectPoolManager.Instance.GetObjectFromPool(droppedItemPoolIndex);
            droppedItem.transform.position = spawnPosition;

            DropppedItem droppedItemComp = droppedItem.GetComponent<DropppedItem>();
            droppedItemComp.Initialize(itemID, count);
            return droppedItemComp;
        }
        return null;
    }

    public DropppedItem SpawnDroppedItemInWorld(ItemInstance itemInstance, Vector3 spawnPosition)
    {
        if(itemInstance == null || itemInstance.ItemDefinition == null || !itemInstance.ItemDefinition.IsValidItem())
        {
            Debug.LogWarning("Tried to spawn invalid item.");
            return null;
        }
        int itemID = itemInstance.ItemDefinition.itemID;
        int count = itemInstance.stackCount;
        return SpawnDroppedItemInWorld(itemID, count, spawnPosition);
    }

    public void ReleaseDroppedItemToPool(DropppedItem item)
    {
        ObjectPoolManager.Instance.ReleaseObjectToPool(droppedItemPoolIndex, item.gameObject);
    }

    private void LoadAllResources()
    {
        ResourceSubsystem.Instance.LoadAllResources(currentGameMapData.resources);
    }
}
