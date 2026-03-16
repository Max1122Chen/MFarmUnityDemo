using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;


public class GameInstance : Singleton<GameInstance>
{
    [Header("Game Settings")]
    [SerializeField] public GameSettings gameSettings = new GameSettings();
    
    [Header("Game Save Data List")]
    // Hold the list of game save data for all save files, this SO will be used to store the game save data for all save files, and we will use it to load and save game data.
    [SerializeField] private GameSaveDataList_SO gameSaveDataListSO;

    [Header("Current Game Save Data")]
    // Reference to the current game save data.
    [SerializeField] GameSaveData currentGameSaveData;

    public PlayerController localPlayer;

    private GameObject MainCanvasObject;

    protected override void Awake()
    {
        base.Awake();



    }
    

    public void Start()
    {   
        if(gameSettings.removeSaveDataOnStart)
        {
            gameSaveDataListSO.gameSaveDataList.Clear();
        }

        // TODO: we will truly enter game when we choose a specific save file to load, for now we will just directly enter game when we start the game, and we will implement the save/load system later.
        if(gameSaveDataListSO.gameSaveDataList.Count == 0)
        {
            CreateNewSaveGameData("Player");
        }
        else
        {
            currentGameSaveData = gameSaveDataListSO.gameSaveDataList[0]; // For now we will just load the first save file in the list, and we will implement the save/load system later to allow player to choose which save file to load.
        }

        InitializeGameSubsystems();

        // Find GO with tag "MainCanvas"
        MainCanvasObject = GameObject.FindGameObjectWithTag("MainCanvas");

        // Find the player in the scene and set the localPlayer reference, we will use this reference to pass to the vendor when we interact with the vendor, so we can process the buy/sell requests in the vendor based on the player data.
        localPlayer = FindObjectOfType<PlayerController>();
        
        if(MainCanvasObject == null)
        {
            Debug.LogError("MainCanvas GameObject with tag 'MainCanvas' not found in the scene.");
        }
        StartToPlay(0);
    }

    // Called after we choose a save file to load or start a new game, this function will initialize the game subsystems and enter the game.
    public void StartToPlay(int saveIndex)
    {
        LoadGame(saveIndex);
    }

    // Initialize all game subsystems. This will only be called once when the application starts.
    public void InitializeGameSubsystems()
    {
        List<GameObject> objectPoolPrefabs = new List<GameObject>();
        Dictionary<ParticleEffectType, int> particleEffectTypeMapping = new Dictionary<ParticleEffectType, int>();

        // Initialize the ParticleEffectManager
        foreach (ParticleEffectDefinition effectDef in ParticleEffectManager.Instance.particleEffectDefinitions)
        {
            if (effectDef.particleEffectPrefab != null)
            {
                objectPoolPrefabs.Add(effectDef.particleEffectPrefab);
                particleEffectTypeMapping[effectDef.effectType] = objectPoolPrefabs.Count - 1; // Map the ParticleEffectType to the index of its prefab in the object pool list
            }
        }
        ParticleEffectManager.Instance.Initialize(particleEffectTypeMapping);

        // Initialize the InventorySubsystem
        InventorySubsystem.Instance.Initialize();

        // Initialize the GameMapSubsystem
        GameMapSubsystem.Instance.Initialize(objectPoolPrefabs.Count); // Pass the current count of object pool prefabs as the index for the dropped item prefab
        objectPoolPrefabs.Add(GameMapSubsystem.Instance.droppedItemPrefab);


        // Initialize the ResourceSubsystem
        ResourceSubsystem.Instance.Initialize();
        // objectPoolPrefabs.Add(ResourceSubsystem.Instance.resourcedefaultPrefab);
        // TODO: use object pool to optimize resource generation and destruction in the future, currently we are still using Instantiate and Destroy for resources, so we will not add the resource prefab to the object pool for now, but we will keep this in mind for future optimization.


        // Initialize the NPCSubsystem
        NPCSubsystem.Instance.Initialize();

        // Initialize the EconomySubsystem
        EconomySubsystem.Instance.Initialize();

        // Initialize the ObjectPoolManager
        ObjectPoolManager.Instance.Initialize(objectPoolPrefabs);
    }

    // Game Saving & Loading
    // TODO: maybe we will provide more info when create a new save game data, such as player name, etc.
    public void CreateNewSaveGameData(string playerName)
    {
        GameSaveData newGameSaveData = new GameSaveData();
        newGameSaveData.saveIndex = gameSaveDataListSO.gameSaveDataList.Count; // Set the save index to the current count of save data in the list
        newGameSaveData.currentScene = gameSettings.initialSceneName;

        CreateNewPlayerSaveData(newGameSaveData, playerName);

        GameMapSubsystem.Instance.HandleCreateNewGameSaveData(newGameSaveData);    

        NPCSubsystem.Instance.HandleCreateNewGameSaveData(newGameSaveData);

        gameSaveDataListSO.gameSaveDataList.Add(newGameSaveData);
    }

    public void CreateNewPlayerSaveData(GameSaveData gameSaveData, string playerName)
    {
        PlayerSaveData playerSaveData = new PlayerSaveData();
        playerSaveData.playerName = playerName;
        gameSaveData.playerSaveData = playerSaveData;

        playerSaveData.position = gameSettings.playerInitialPosition;
        playerSaveData.money = gameSettings.startingMoney;
        
        foreach (InventoryItemSaveData itemData in gameSettings.playerStartingItems)
        {
            playerSaveData.playerInventory.Add(itemData);
        }
    }

    public void SaveGame(int saveIndex)
    {
        // Currently some of the game data will be directly saved to the currentGameSaveData when the data changes.
        // e.g. player's data
        // except for the game map data, we will save the game map data when we save the game, because the game map data is relatively large and we don't want to save it every time it changes, so we will save it when we save the game.
    }

    public void LoadGame(int saveIndex)
    {
        // TODO: Implement the game loading logic, such as loading the game map data, player data, NPC data, etc. We will use the currentGameSaveData to load the game data.
        currentGameSaveData = gameSaveDataListSO.gameSaveDataList[saveIndex];

        // Load Player data
        localPlayer.Initialze(currentGameSaveData.playerSaveData);

        // Load the game map data and load the scene.
        GameMapSubsystem.Instance.LoadGameMapSaveDataList(currentGameSaveData.gameMapSaveDataList);

        // Randomly generate resources on the map if it's the player's first time playing the game.
        if(currentGameSaveData.firstTimePlaying)
        {
            currentGameSaveData.firstTimePlaying = false;
            List<System.Tuple<string, int>> generatedResources = ResourceSubsystem.Instance.RandomlyGenerateResourcesOnAllMaps(gameSettings.initialResourceInfos);
            foreach(var resource in generatedResources)
            {
                Debug.Log("Generated resource with ID: " + resource.Item1 + " on map: " + resource.Item2);
            }
        }
        GameMapSubsystem.Instance.StartCoroutine(GameMapSubsystem.Instance.SwitchScene(currentGameSaveData.currentScene));

        // Load NPC data and spawn NPCs in the world based on the NPC save data.

        NPCSubsystem.Instance.LoadAllNPCs(currentGameSaveData.npcSaveDataList, currentGameSaveData.currentScene);
    }

    // Utility function to spawn game objects in the world.
    public GameObject SpawnGameObjectInWorld(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        return Instantiate(prefab, spawnPosition, spawnRotation);
    }

    public GameObject SpawnGameObjectInWorld(GameObject prefab, Transform parent)
    {
        return Instantiate(prefab, parent);
    }

    public GameObject CreateUI(GameObject uiPrefab, Vector2 position, Transform parent = null)
    {
        var uiGO = Instantiate(uiPrefab, Vector3.zero, Quaternion.identity);
        uiGO.transform.localPosition = position;
        if (parent == null)
        {
            uiGO.transform.SetParent(MainCanvasObject.transform, false);
        }
        else
        {
            uiGO.transform.SetParent(parent, false);
        }
        return uiGO;
    }
}
