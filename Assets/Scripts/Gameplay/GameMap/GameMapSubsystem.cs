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

    public Action onNewSceneLoaded;
    
    // Game Map Data
    [SerializeField] private List<GameMapData_SO> gameMapDataList = new List<GameMapData_SO>();

    // Key: sceneName, Value: GameMapData_SO
    private Dictionary<string, GameMapData_SO> gameMapDataDict = new Dictionary<string, GameMapData_SO>();

    [SerializeField] private GameMapData_SO currentGameMapData;
    public Dictionary<Vector2Int, TileInfo> currentTileInfoDict { get ; private set; } = new Dictionary<Vector2Int, TileInfo>();
    [SerializeField] private string currentSceneName;

    public Grid currentGrid { get; private set; }
    public GameMapData_SO CurrentGameMapData => currentGameMapData;

    [Header(" Dropped Item Prefab")]
    public GameObject droppedItemPrefab;
    private List<DropppedItem> droppedItemsInCurrentScene = new List<DropppedItem>();


    void Start()
    {
        InitializeGameMapDataDict();

        if(!string.IsNullOrEmpty(initialSceneName))
        {
            StartCoroutine(SwitchScene(initialSceneName));
        }
        else
        {
            Debug.LogError("Initial scene name is not set in GameMapSubsystem.");
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

    private IEnumerator LoadScene(string sceneName)
    {
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

        onNewSceneLoaded?.Invoke();

        Debug.Log("Loaded scene: " + sceneName);
    }

    private IEnumerator UnloadScene(string sceneName)
    {
        if(!String.IsNullOrEmpty(sceneName))
        {
            SaveGameMapData();

            yield return SceneManager.UnloadSceneAsync(sceneName);
            currentGameMapData = null;
            Debug.Log("Unloaded scene: " + sceneName);
        }
        else
        {
            Debug.LogWarning("Tried to unload a scene with null or empty name.");
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

    public void PlaceFurniture(TileInfo tile, ItemDefinition itemDef)
    {
        tile.hasThing = true;
        Vector3 worldPos = currentGrid.GetCellCenterWorld((Vector3Int)tile.position);
        GameObject furniturePrefab = InventorySubsystem.Instance.GetPlacablePrefab(itemDef.itemID);
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
            }
        }
    }

    private void SaveAllResources()
    {
        currentGameMapData.resources = ResourceSubsystem.Instance.SaveAllResources();
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
            SpawnDroppedItemInWorld(itemData.itemID, itemData.itemCount, itemData.position);
        }
    }

    public void SpawnDroppedItemInWorld(int itemID, int count, Vector3 spawnPosition)
    {
        ItemDefinition itemDef = InventorySubsystem.Instance.GetItemDefinition(itemID);
        SpawnDroppedItemInWorld(new ItemInstance(itemDef, count), spawnPosition);
    }

    public void SpawnDroppedItemInWorld(ItemInstance itemInstance, Vector3 spawnPosition)
    {
        if(itemInstance == null || itemInstance.ItemDefinition == null || !itemInstance.ItemDefinition.IsValidItem())
        {
            Debug.LogWarning("Tried to spawn invalid item.");
            return;
        }
        var itemGO = GameInstance.Instance.SpawnGameObjectInWorld(droppedItemPrefab, spawnPosition, Quaternion.identity);

        DropppedItem droppedItemComponent = itemGO.GetComponent<DropppedItem>();
        droppedItemComponent.itemID = itemInstance.ItemDefinition.itemID;
        droppedItemComponent.itemCount = itemInstance.stackCount;
    }

    private void LoadAllResources()
    {
        ResourceSubsystem.Instance.LoadAllResources(currentGameMapData.resources);
    }
}
