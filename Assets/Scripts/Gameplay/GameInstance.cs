using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using InventorySystem;

public class GameInstance : Singleton<GameInstance>
{
    [Header("Game Settings")]
    [SerializeField] public GameSettings gameSettings = new GameSettings();
    
    private GameObject MainCanvasObject;

    protected override void Awake()
    {
        base.Awake();



    }
    

    public void Start()
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

        // Initialize the ObjectPoolManager
        ObjectPoolManager.Instance.Initialize(objectPoolPrefabs);


        // Find GO with tag "MainCanvas"
        MainCanvasObject = GameObject.FindGameObjectWithTag("MainCanvas");
        
        if(MainCanvasObject == null)
        {
            Debug.LogError("MainCanvas GameObject with tag 'MainCanvas' not found in the scene.");
        }


    }

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
