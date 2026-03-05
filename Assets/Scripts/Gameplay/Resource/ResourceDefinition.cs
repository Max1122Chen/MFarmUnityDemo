using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

[System.Serializable]
public enum ResourceType
{
    None,
    Tree,
    Ore,
    Crop,
    Other
}

[System.Serializable]
public class GrowthStageInfo
{
    public Sprite stageSprite;
    public int timeToNextStage = 0;   // Time required to advance to the next growth stage, measured in game minute.
}

[System.Serializable]
public class ProductionInfo
{
    public int producedItemID = -1;
    public int growthStageIndex = -1; // -1 means the last growth stage(ripe stage)
    public int producedItemMinCount = 0;
    public int producedItemMaxCount = 0;
}

[System.Serializable]
public class GatheringRequirement
{
    public ItemType requiredToolType = ItemType.None;
    public int requiredActionCount = 1;
    public bool canGenerateProduct = false;
    
}

[System.Serializable]
public enum ResourcePrefabType
{
    OnRipe,
    AfterGathering
}

[System.Serializable]
public class ResourcePrefabInfo
{
    public ResourcePrefabType prefabType;
    public int resourceID = -1;
}


[System.Serializable]
public class ResourceDefinition
{
    public int resourceID = -1;     // if the resource is related to an item, resourceID should be the same as the itemID of the related item
    public ResourceType resourceType;

    [Header("Resource Prefab")]
    public GameObject resourcePrefab = null; // null means use the default simple prefab.

    [Header("Growth Stages")]
    public List<GrowthStageInfo> growthStages;

    [Header("Production")]
    public List<ProductionInfo> productionInfos;
    public bool dropProductsOnGround = false; // Whether the produced items will be dropped on the ground when the resource is gathered or becomes ripe, or directly added to the gatherer's inventory if the gatherer has one. If the gatherer does not have an inventory, the produced items will be dropped on the ground regardless of this setting.

    [Header("Gathering Requirements")]
    public List<GatheringRequirement> gatheringRequirements;

    [Header("Gathering Behavior")]
    public ParticleEffectType gatheringParticleEffectType; // The particle effect to play when the resource is being gathered.
    public bool canBeGatheredMultipleTimes = false; // Whether the resource can be gathered multiple times. If false, the resource will be destroyed after being gathered once. If true, the resource will not be destroyed after being gathered, and it can produce products and be gathered again after a certain regrowth time.
    public int regrowthTime = 0; // Time required for the resource to regrow to the next growth stage after being gathered, measured in game minute. This is only relevant if canBeGatheredMultipleTimes is true.

    [Header("Object Generation")]
    public List<ResourcePrefabInfo> generatedObjectPrefabs; // List of prefabs to generate when the resource is gathered or becomes ripe. The specific prefab to generate will be determined by the ResourcePrefabType field in ResourcePrefabInfo.

    [Header("Collision")]
    public bool hasCollision = false; 
    public Vector2 colliderSize = Vector2.one; // Default collider size, can be overridden by the prefab's own collider if the prefab has one.
}
