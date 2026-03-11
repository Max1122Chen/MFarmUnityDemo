using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation.AStar
{

    public class AStarDebugger : MonoBehaviour
    {
        [Header("Visual Elements")]
        public GameObject nodePrefab;
        Dictionary<Node, DebugNode> nodeVisuals = new Dictionary<Node, DebugNode>();
        private AStarNavigationAgent agent;
        private Vector2Int startPos;
        private GameObject startNodeVisual;
        private Vector2Int targetPos;
        private GameObject targetNodeVisual;

        public void Awake()
        {
            agent = GetComponent<AStarNavigationAgent>();
            agent.onNewRoundStarted += DrawFindingPathProgress;
        }
        public void Update()
        {
            // Choose the start pos
            if(Input.GetMouseButtonDown(0))
            {
                if(!agent.isFindingPath)
                {
                    if(startNodeVisual != null)
                    {
                        Destroy(startNodeVisual);
                    }
                    startPos = ChoosePosition();
                    Debug.Log($"Start Pos Chosen: {startPos}");
                    startNodeVisual = DrawStartOrTargetNode(startPos, DebugNodeType.Start);
                    ClearVisuals();
                }
            }
            // Choose the target pos
            if(Input.GetMouseButtonDown(1))
            {
                if(!agent.isFindingPath)
                {
                    if(targetNodeVisual != null)
                    {
                        Destroy(targetNodeVisual);
                    }
                    targetPos = ChoosePosition();
                    Debug.Log($"Target Pos Chosen: {targetPos}");
                    targetNodeVisual = DrawStartOrTargetNode(targetPos, DebugNodeType.Target);
                    ClearVisuals();
                }
            }
            if(Input.GetMouseButtonDown(2))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(mousePos);
                Debug.Log($"Tile Info at mouse position: {tileInfo}");
            }
            // Start finding path
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(!agent.isFindingPath)
                {
                    if(startPos != Vector2Int.zero && targetPos != Vector2Int.zero)
                    {
                        ClearVisuals();
                        List<Node> path = agent.FindPath(startPos, targetPos, GameMapSubsystem.Instance.currentGameMapSaveData.mapSize);
                    }
                }
                else
                {
                    // If we're already finding a path, we can choose to either pause/resume the pathfinding process. For now, let's just toggle the paused state.
                    Debug.Log("Toggling pathfinding paused state...");
                    agent.paused = !agent.paused;
                }   
            }
        }

        private Vector2Int ChoosePosition()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(mousePos);

            // We need to 
            return tileInfo != null ? tileInfo.position: Vector2Int.zero;
        }
        
        private void DrawFindingPathProgress()
        {
            Debug.Log("Drawing pathfinding progress...");
            Dictionary<Vector2Int, Node> allNodes = agent.allNodes;
            HashSet<Node> openSet = agent.openSet;
            HashSet<Node> closedSet = agent.closedSet;

            foreach(Node node in allNodes.Values)
            {
                // Skip the start and target nodes, since they will be drawn with their own specific colors and visuals.
                if(node.position == startPos || node.position == targetPos)
                {
                    continue;
                }
                // The node can be in the open set, closed set, or neither (not yet processed). We can use different colors or visuals to represent these states.
                if(openSet.Contains(node))
                {
                    if(node.walkable)
                    {
                        DrawNode(node, DebugNodeType.Potential);
                    }
                    else
                    {
                        DrawNode(node, DebugNodeType.Obstacle);
                    }
                }
                else if(closedSet.Contains(node))
                {
                    DrawNode(node, DebugNodeType.Visited);
                }
                else
                {
                    DrawNode(node, DebugNodeType.Obstacle);
                }
            }
        }

        private GameObject DrawNode(Node node, DebugNodeType type)
        {
            if(!nodeVisuals.ContainsKey(node))
            {
                TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByGridPos(node.position);
                Vector2 worldPos = GameMapSubsystem.Instance.GetWorldPositionByTileInfo(tileInfo);
                GameObject newNode = GameInstance.Instance.SpawnGameObjectInWorld(nodePrefab, worldPos, Quaternion.identity);
                DebugNode debugNode = newNode.GetComponent<DebugNode>();

                // The node has not been visualized yet, create a new one
                if(debugNode != null)
                {
                    debugNode.UpdateVisual(type);
                    nodeVisuals[node] = debugNode;
                    return newNode;
                }
            }
            // The node has already been visualized, just update its visual state
            else
            {
                DebugNode debugNode = nodeVisuals[node];
                if(debugNode != null)
                {
                    debugNode.UpdateVisual(type);
                    return debugNode.gameObject;
                }
            }
            return null;
        }

        private GameObject DrawStartOrTargetNode(Vector2Int position, DebugNodeType type)
        {
            TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByGridPos(position);
            Vector2 worldPos = GameMapSubsystem.Instance.GetWorldPositionByTileInfo(tileInfo);
            GameObject newNode = GameInstance.Instance.SpawnGameObjectInWorld(nodePrefab, worldPos, Quaternion.identity);
            DebugNode debugNode = newNode.GetComponent<DebugNode>();

            if(debugNode != null)
            {
                debugNode.UpdateVisual(type);
                return newNode;
            }
            return null;
        }

        private void ClearVisuals()
        {
            List<DebugNode> visualsToDestroy = new List<DebugNode>(nodeVisuals.Values);
            foreach(DebugNode debugNode in nodeVisuals.Values)
            {
                visualsToDestroy.Add(debugNode);
            }
            nodeVisuals.Clear();
            foreach(DebugNode debugNode in visualsToDestroy)
            {
                if(debugNode != null)
                {
                    Destroy(debugNode.gameObject);
                }
            }
        }
    }
}

