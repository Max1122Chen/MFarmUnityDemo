using System;
using System.Collections;
using System.Collections.Generic;
using TimeSystem;
using UnityEngine;

public class ResourceSubsystem : Singleton<ResourceSubsystem>
{
    [Header("Resource Data")]
    public ResourceDataList_SO resourceDataList_SO;
    public TreeDataList_SO treeDataList_SO;
    private Dictionary<int, ResourceDefinition> resourceDefinitions = new Dictionary<int, ResourceDefinition>(); // Key: resourceID, Value: ResourceDefinition
    private Dictionary<int, TreeDefinition> treeDefinitions = new Dictionary<int, TreeDefinition>(); // Key: treeID, Value: TreeDefinition
    public Dictionary<Vector2Int,Resource> registeredResources = new Dictionary<Vector2Int, Resource>();

    public List<Resource> resourcesNeedToBeUpdated = new List<Resource>();

    [Header("Prefabs")]
    public GameObject resourcedefaultPrefab; // Prefab for general resource. This prefab should contain the Resource component and other necessary components such as SpriteRenderer component.

    protected override void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        TimeSubsystem.Instance.onMinutePassed += UpdateResourcesGrowth;
    }
    public void Initialize()
    {
        foreach (ResourceDefinition resourceDef in resourceDataList_SO.resourceDataList)
        {
            if (!resourceDefinitions.ContainsKey(resourceDef.resourceID))
            {
                resourceDefinitions.Add(resourceDef.resourceID, resourceDef);
            }
        }

        foreach (TreeDefinition treeDef in treeDataList_SO.treeDataList)
        {
            if (!treeDefinitions.ContainsKey(treeDef.treeID))
            {
                treeDefinitions.Add(treeDef.treeID, treeDef);
            }
        }
    }

    [System.Serializable]
    public struct ResourceGenerationInfo
    {
        public int resourceID;
        public float weight;
    }
    public List<Tuple<string, int>> RandomlyGenerateResourcesOnAllMaps(List<ResourceGenerationInfo> generationInfo)
    {
        // Key: sceneName, Value: number of resources generated on that map. 
        List<Tuple<string, int>> generatedResources = new List<Tuple<string, int>>();
        foreach (GameMapSaveData gameMapSaveData in GameMapSubsystem.Instance.gameMapSaveDataList)
        {
            int generatedCount = RandomlyGenerateResourcesOnMap(generationInfo, gameMapSaveData.SceneName);
            generatedResources.Add(new Tuple<string, int>(gameMapSaveData.SceneName, generatedCount));
        }
        return generatedResources;
    }

    public int RandomlyGenerateResourcesOnMap(List<ResourceGenerationInfo> generationInfo, string sceneName, int numSamplesBeforeRejection = 30)
    {
        GameMapSaveData gameMapSaveData = GameMapSubsystem.Instance.GetGameMapSaveData(sceneName);
        int generatedCount = 0;
        if(gameMapSaveData != null)
        {
            List<Vector2> randomPoints = PoissonDiskSampler.GeneratePoints(GameInstance.Instance.gameSettings.resourceGenerateRadius, new Vector2(gameMapSaveData.mapSize.x, gameMapSaveData.mapSize.y), numSamplesBeforeRejection);
            foreach (Vector2 point in randomPoints)
            {
                Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(point.x) + gameMapSaveData.mapOffset.x, Mathf.FloorToInt(point.y) + gameMapSaveData.mapOffset.y);
                TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByGridPos(gridPos, sceneName);
                if (tileInfo != null && tileInfo.diggable && !tileInfo.isOccupied)
                {
                    // Pick a random kind of resource to generate from the provided resourceIDs list. 
                    int totalWeight = 0;
                    foreach (ResourceGenerationInfo info in generationInfo)                    {
                        totalWeight += (int)(info.weight * 100); // Multiply by 100 to convert the weight to an integer for better precision.
                    }
                    int randomIndex = UnityEngine.Random.Range(0, totalWeight);
                    int cumulativeWeight = 0;
                    int resourceID = -1;
                    foreach (ResourceGenerationInfo info in generationInfo)
                    {
                        cumulativeWeight += (int)(info.weight * 100);
                        if (randomIndex < cumulativeWeight)
                        {
                            resourceID = info.resourceID;
                            break;
                        }
                    }
                    ResourceDefinition resourceDef = GetResourceDefinition(resourceID);
                    if(resourceDef != null)
                    {
                        int growthStageIndex = UnityEngine.Random.Range(0, resourceDef.growthStages.Count); // Randomly assign a growth stage index to the generated resource.
                        int grownTime = 0;
                        for (int i = 0; i < growthStageIndex; i++)
                        {
                            grownTime += resourceDef.growthStages[i].timeToNextStage;
                        }
                        if(growthStageIndex > 0)
                        {
                            grownTime += UnityEngine.Random.Range(0, resourceDef.growthStages[growthStageIndex].timeToNextStage); // Randomly assign a growth time counter within the current growth stage to make the generated resources look more natural.
                        }
                        ResourceSaveData saveData = GenerateResource(tileInfo, resourceDef, growthStageIndex, grownTime);
                        gameMapSaveData.resources.Add(saveData);
                        generatedCount++;
                        Debug.Log(tileInfo.gridPos + ": Generated resource with ID " + resourceID + " at growth stage index " + growthStageIndex + " with grown time " + grownTime);
                    }
                }
            }   
        }
        return generatedCount;
    }

    public void RegisterResource(Resource resource)
    {
        // In case the resource is generated without providing tileInfo, we can get the tileInfo based on the resource's world position and set it to the resource, so that we can keep track of the resource based on its tile position.
        TileInfo tileInfo = resource.tileInfo;
        if(tileInfo == null)
        {
            tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(resource.transform.position);
            resource.SetTileInfo(tileInfo);
        }

        Vector2Int position = resource.tileInfo.gridPos;
        if (!registeredResources.ContainsKey(position))
        {
            registeredResources.Add(position, resource);
            // Debug.Log($"Registered resource {resource.name} at position {position}.");

            if(resource.resourceDef.growthStages.Count > 2)
            {
                resourcesNeedToBeUpdated.Add(resource);
            }
        }
    }

    public void UnregisterResource(Resource resource)
    {
        Vector2Int position = resource.tileInfo.gridPos;
        if (registeredResources.ContainsKey(position))
        {
            registeredResources.Remove(position);
        }

        if(resourcesNeedToBeUpdated.Contains(resource))
        {
            resourcesNeedToBeUpdated.Remove(resource);
        }
    }

    public Resource GetResourceByPosition(Vector2Int position)
    {
        if (registeredResources.ContainsKey(position))
        {
            return registeredResources[position];
        }
        else
        {
            return null;
        }
    }

    public void UpdateResourcesGrowth(int passedMinutes)
    {
        foreach (Resource resource in resourcesNeedToBeUpdated)
        {
            resource.Grow(passedMinutes);
        }
    }

    public ResourceDefinition GetResourceDefinition(int resourceID)
    {
        if (resourceDefinitions.ContainsKey(resourceID))
        {
            return resourceDefinitions[resourceID];
        }
        else
        {
            Debug.LogError($"ResourceDefinition with ID {resourceID} not found.");
            return null;
        }
    }

    public TreeDefinition GetTreeDefinition(int treeID)
    {
        if (treeDefinitions.ContainsKey(treeID))
        {
            return treeDefinitions[treeID];
        }
        else
        {
            Debug.LogError($"TreeDefinition with ID {treeID} not found.");
            return null;
        }
    }

    public ResourceSaveData GenerateResource(TileInfo tileInfo, ResourceDefinition resourceDef, int growthStageIndex, int grownTime)
    {
        ResourceSaveData saveData = new ResourceSaveData(resourceDef.resourceID, tileInfo.gridPos, growthStageIndex, grownTime);

        tileInfo.isOccupied = true; // Mark the tile as occupied when generating a resource on it.

        return saveData;
    }
    public Resource SpawnResource(TileInfo tileInfo,int resourceID, int growthStageIndex, int grownTime)
    {
        ResourceDefinition resourceDef = GetResourceDefinition(resourceID);
        if(resourceDef == null)
        {
            return null;
        }
        return SpawnResource(tileInfo, resourceDef, growthStageIndex, grownTime);
    }

    public Resource SpawnResource(TileInfo tileInfo, ResourceDefinition resourceDef, int growthStageIndex, int grownTime)
    {
        Grid currentGrid = GameMapSubsystem.Instance.currentGrid;
        Vector3 worldPos = currentGrid.GetCellCenterWorld((Vector3Int)tileInfo.gridPos);

        GameObject prefabToUse = resourceDef.resourcePrefab != null ? resourceDef.resourcePrefab : resourcedefaultPrefab;

        GameObject resourceObj = GameInstance.Instance.SpawnGameObjectInWorld(prefabToUse, worldPos, Quaternion.identity);
        Resource resource = resourceObj.GetComponent<Resource>();
        resource.Initialize(resourceDef.resourceID, growthStageIndex, grownTime);

        tileInfo.isOccupied = true;
        return resource;
    }

    // Saving and loading
    public void SaveAllResources(List<ResourceSaveData> saveDataList)
    {
        // Clear existing data.
        saveDataList.Clear();

        foreach (var kvp in registeredResources)
        {
            Resource resource = kvp.Value;
            ResourceSaveData saveData = new ResourceSaveData(resource.resourceID, resource.transform.position, resource.currentGrowthStageIndex, resource.growthTimeCounter);
            saveDataList.Add(saveData);
        }

        // Clear the registered resources list after saving to free up memory. We will re-register the resources when we load them back in.
        registeredResources.Clear(); 
    }

    public void LoadAllResources(List<ResourceSaveData> saveDataList)
    {
        foreach (ResourceSaveData saveData in saveDataList)
        {
            TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(saveData.position);
            if (tileInfo != null)
            {
                RegisterResource(SpawnResource(tileInfo, saveData.resourceID, saveData.growthStageIndex, saveData.growthTimeCounter));
            }
            else
            {
                Debug.LogError($"Failed to load resource at position {saveData.position} because no corresponding tile was found.");
            }
        }
        saveDataList.Clear(); // Clear the save data list after loading to free up memory.
    }

    
}
