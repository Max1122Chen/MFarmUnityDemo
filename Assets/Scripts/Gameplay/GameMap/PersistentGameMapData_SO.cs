using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[System.Serializable]
public enum TileType
{
    None,
    Diggable,
    ItemDroppable,
    FurniturePlacable,
    NPCObstacle,
    TileMapRangeMarker // This is a special tile type that is used to mark the range of the tilemap
}

[System.Serializable]
public class TileInfo
{
    public Vector2Int gridPos;
    public Vector2 worldPos;

    // Tile Properties
    public bool diggable = false;
    public bool itemDroppable = false;
    public bool thingPlacable = false;

    // Tile State
    public bool isWatered = false;
    public bool isOccupied = false;
    public int daySinceDug = -1;
    public int seedID = -1;

    public override string ToString()
    {
        return $"Pos: {gridPos}, Diggable: {diggable}, ItemDroppable: {itemDroppable}, FurniturePlacable: {thingPlacable}, DaySinceDug: {daySinceDug}, SeedID: {seedID}, isOccupied: {isOccupied}";
    }

    public TileInfo()
    {}
    public TileInfo(TileInfo other)
    {
        gridPos = other.gridPos;
        worldPos = other.worldPos;
        diggable = other.diggable;
        itemDroppable = other.itemDroppable;
        thingPlacable = other.thingPlacable;
        isWatered = other.isWatered;
        isOccupied = other.isOccupied;
        daySinceDug = other.daySinceDug;
        seedID = other.seedID;
    }
}

// The persistent data SO that holds the map data for the scene. Wont be changed during runtime, only used for reference when initialzie a new game.
[CreateAssetMenu(fileName = "PersistentGameMapData_SO", menuName = "GameMap/PersistentGameMapData_SO")]
public class PersistentGameMapData_SO : ScriptableObject
{
    [SceneName] [SerializeField] public string sceneName;

    public string SceneName => sceneName;
    [Header("Tile Info List")]
    public List<TileInfo> tileInfoList = new List<TileInfo>();
    public Vector2Int mapSize;
    public Vector2Int mapOffset;


    [Header("Saved Dynamic Objects Data")]
    public List<DroppedItemSaveData> droppedItems = new List<DroppedItemSaveData>();
    public List<ResourceSaveData> resources = new List<ResourceSaveData>();
    public List<ContainerSaveData> containers = new List<ContainerSaveData>();

}
