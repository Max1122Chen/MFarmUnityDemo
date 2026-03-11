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

    public void RegisterResource(Resource resource)
    {
        // In case the resource is generated without providing tileInfo, we can get the tileInfo based on the resource's world position and set it to the resource, so that we can keep track of the resource based on its tile position.
        TileInfo tileInfo = resource.tileInfo;
        if(tileInfo == null)
        {
            tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(resource.transform.position);
            resource.SetTileInfo(tileInfo);
        }

        Vector2Int position = resource.tileInfo.position;
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
        Vector2Int position = resource.tileInfo.position;
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

    public Resource GenerateResource(TileInfo tileInfo,int resourceID, int growthStageIndex, int grownTime)
    {
        ResourceDefinition resourceDef = GetResourceDefinition(resourceID);
        if(resourceDef == null)
        {
            return null;
        }
        return GenerateResource(tileInfo, resourceDef, growthStageIndex, grownTime);
    }

    public Resource GenerateResource(TileInfo tileInfo, ResourceDefinition resourceDef, int growthStageIndex, int grownTime)
    {
        Grid currentGrid = GameMapSubsystem.Instance.currentGrid;
        Vector3 worldPos = currentGrid.GetCellCenterWorld((Vector3Int)tileInfo.position);

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
                RegisterResource(GenerateResource(tileInfo, saveData.resourceID, saveData.growthStageIndex, saveData.growthTimeCounter));
            }
            else
            {
                Debug.LogError($"Failed to load resource at position {saveData.position} because no corresponding tile was found.");
            }
        }
        saveDataList.Clear(); // Clear the save data list after loading to free up memory.
    }

    
}
