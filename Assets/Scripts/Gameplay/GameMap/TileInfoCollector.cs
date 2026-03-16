using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TileInfoCollector : MonoBehaviour
{
    // gameMapData must be assigned in inspector before editing tilemap, and will be updated when the object is disabled (e.g. when exiting play mode or deselecting the object in editor).
    public PersistentGameMapData_SO gameMapData;
    public TileType tileType;
    private Tilemap currentMap;

    void OnEnable()
    {
        if(!Application.IsPlaying(this))
        {
            currentMap = GetComponent<Tilemap>();
            if(gameMapData != null)
            {
                gameMapData.tileInfoList.Clear();
            }
        }
    }

    void OnDisable()
    {
        if(!Application.IsPlaying(this))
        {
            currentMap = GetComponent<Tilemap>();
            if(gameMapData != null)
            {
                UpdateTileInfo();
                
#if UNITY_EDITOR
                 UnityEditor.EditorUtility.SetDirty(gameMapData);
                if(gameMapData != null)
                {
                    UnityEditor.EditorUtility.SetDirty(gameMapData);
                }
#endif
            }
        }
    }
    
    private void UpdateTileInfo()
    {
        currentMap.CompressBounds();
        Vector3Int startPos = currentMap.cellBounds.min;
        Vector3Int endPos = currentMap.cellBounds.max;

        if(tileType == TileType.TileMapRangeMarker)
        {
            gameMapData.mapSize = new Vector2Int(endPos.x - startPos.x, endPos.y - startPos.y);
            gameMapData.mapOffset = new Vector2Int(startPos.x, startPos.y);
        }

        for(int x = startPos.x; x < endPos.x; x++)
        {
            for(int y = startPos.y; y < endPos.y; y++)
            {
                Vector3Int currentPos = new Vector3Int(x, y, 0);
                TileBase tile = currentMap.GetTile(currentPos);

                if(tile != null)
                {
                    Vector2Int tilePos = new Vector2Int(x, y);
                    TileInfo tileInfo;

                    if (gameMapData.tileInfoList.Exists(t => t.gridPos == tilePos))
                    {
                        tileInfo = gameMapData.tileInfoList.Find(t => t.gridPos == tilePos);
                    }
                    else
                    {
                        tileInfo = new TileInfo();
                        tileInfo.gridPos = tilePos;
                        tileInfo.worldPos = currentMap.CellToWorld(currentPos);
                        gameMapData.tileInfoList.Add(tileInfo);
                    }

                    switch (tileType)
                    {
                        case TileType.Diggable:
                            tileInfo.diggable = true;
                            break;
                        case TileType.ItemDroppable:
                            tileInfo.itemDroppable = true;
                            break;
                        case TileType.FurniturePlacable:
                            tileInfo.thingPlacable = true;
                            break;
                        case TileType.NPCObstacle:
                            tileInfo.isOccupied = true;
                            break;
                        case TileType.TileMapRangeMarker:
                            break;
                    }
                    // Debug.Log($"Updated tile info at {tilePos}: {tileInfo}");
                }
            }
        }
        Debug.Log($"Finished updating tile info for scene {gameMapData.SceneName}. Total tiles: {gameMapData.tileInfoList.Count}");
    }
}
