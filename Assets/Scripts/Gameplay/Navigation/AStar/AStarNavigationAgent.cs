using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// In case we have other types of pathfinding algorithms in the future, we can put the common properties and methods in this base class.
namespace Navigation.AStar
{
    [System.Serializable]
    public class Node : IComparable<Node>
    {
        public Vector2Int position;
        public bool walkable = true;

        // A* specific properties
        public int gCost; // Cost from start node to this node
        public int hCost; // Heuristic cost from this node to end node
        public int fCost => gCost + hCost; // Total cost

        public Node parent; // To trace back the path

        public Node(Vector2Int pos, bool walkable)
        {
            this.position = pos;
            this.walkable = walkable;
            this.gCost = int.MaxValue; // Initialize gCost to a very high value, it will be updated during the algorithm.
            this.hCost = 0;
            this.parent = null;
        }

        public int CompareTo(Node other)
        {
            // First compare by fCost, if they are equal, then compare by hCost to break ties (prefer nodes that are closer to the target).
            int result = fCost.CompareTo(other.fCost);
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }
            return result;
        }
    }

    public class AStarNavigationAgent : MonoBehaviour
    {
        // Cache the nodes for potential use in future pathfinding calls, so we don't have to recreate them every time we find a path.
        public Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();
        public HashSet<Node> openSet = new HashSet<Node>();
        public HashSet<Node> closedSet = new HashSet<Node>();
        public bool paused = false;
        public bool isFindingPath = false;     // Flag to indicate if a pathfinding operation is currently in progress.

        // Debug
        public Action onNewRoundStarted;

        public void GenerateNavigationData(Vector2Int startPos, string startScene, Vector2Int targetPos, string targetScene)
        {
            
        }

        // Find a path in 
        public List<Node> FindPath(Vector2Int startPos, Vector2Int targetPos, string sceneName)
        {
            if(isFindingPath)
            {
                // If we're already finding a path, we can choose to either return null or throw an exception. For now, let's just return null.
                return null;
            }

            Debug.Log("Starting pathfinding...");
            isFindingPath = true;

            // Clear the sets for a new pathfinding operation.
            allNodes.Clear();
            openSet.Clear();
            closedSet.Clear();

            Node startNode = !GameMapSubsystem.Instance.GetTileInfoByGridPos(startPos, sceneName).isOccupied ? new Node(startPos, true) : null;
            Node targetNode = !GameMapSubsystem.Instance.GetTileInfoByGridPos(targetPos, sceneName).isOccupied ? new Node(targetPos, true) : null;

            if(startNode == null || targetNode == null)
            {
                // If either the start or target node is not walkable, we can't find a path.
                isFindingPath = false;
                return null;
            }

            startNode.gCost = 0;
            startNode.hCost = GetHeuristicCost(startNode, targetNode);
            openSet.Add(startNode);

            while(openSet.Count > 0)
            {

#if UNITY_EDITOR
                onNewRoundStarted?.Invoke();
#endif
                Node currentNode = null;
                foreach(Node node in openSet)
                {
                    // Find the node in the open set with the lowest fCost (and hCost as a tiebreaker)
                    if(currentNode == null || currentNode.CompareTo(node) > 0)
                    {   
                        currentNode = node;
                    }
                }

                // Remove the current node from the open set and add it to the closed set.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Check if we reached the target
                if (currentNode.position == targetPos)
                {
                    List<Node> path = new List<Node>();
                    while (currentNode != null)
                    {
                        path.Add(currentNode);
                        currentNode = currentNode.parent;
                    }
                    path.Reverse(); // Reverse the path to get it from start to target.
                    isFindingPath = false;
                    return path;
                }

                // Then deal with the neighbors of the current node.
                List<Node> neighbors = GetNeighbors(currentNode,sceneName, allNodes);
                foreach(Node neighbor in neighbors)
                {
                    if(closedSet.Contains(neighbor) || !neighbor.walkable)
                    {
                        // If the neighbor is already in the closed set, skip it.
                        continue;
                    }

                    int tentativeGCost = GetGCost(currentNode, neighbor);

                    if(!openSet.Contains(neighbor) || tentativeGCost < neighbor.gCost)
                    {
                        // If the neighbor is not in the open set, or we found a cheaper path to it, update its costs and parent.
                        neighbor.gCost = tentativeGCost;
                        neighbor.hCost = GetHeuristicCost(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if(!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            isFindingPath = false;
            return null; // No path found

        }

        private List<Node> GetNeighbors(Node node, string sceneName, Dictionary<Vector2Int, Node> allNodes)
        {
            List<Node> neighbors = new List<Node>();

            // Define the possible directions (up, down, left, right, and diagonals)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0),   // Right
                new Vector2Int(-1, 1),  // Up-Left
                new Vector2Int(1, 1),   // Up-Right
                new Vector2Int(-1, -1), // Down-Left
                new Vector2Int(1, -1)   // Down-Right
            };

            foreach(Vector2Int direction in directions)
            {
                Vector2Int neighborPos = node.position + direction;
                Vector2Int neighborArrayPos = neighborPos - GameMapSubsystem.Instance.currentGameMapSaveData.mapOffset;

                // Check if the neighbor position is within the bounds of the grid
                Vector2Int gridSize = GameMapSubsystem.Instance.GetGridSizeOfScene(sceneName);
                if(neighborArrayPos.x >= 0 && neighborArrayPos.x < gridSize.x && neighborArrayPos.y >= 0 && neighborArrayPos.y < gridSize.y)
                {
                    // We dont handle ignorance of obstacles when getting neighbors here, preventing multiple responsibilities for a single function.
                    // TODO: in the future, we need to handle the case where the character can travel across the scene.
                    bool walkable = !GameMapSubsystem.Instance.GetTileInfoByGridPos(neighborPos, sceneName).isOccupied;

                    if(allNodes.ContainsKey(neighborPos))
                    {
                        neighbors.Add(allNodes[neighborPos]);
                    }
                    else
                    {
                        Node neighborNode = new Node(neighborPos, walkable);
                        allNodes[neighborPos] = neighborNode;
                        neighbors.Add(neighborNode);
                    }
                }
            }

            return neighbors;
        }

        private int GetGCost(Node current, Node neighbor)
        {
            // Since the neighbor is adjacent to the current node, 
            int heuristicCost = GetHeuristicCost(current, neighbor);

            return current.gCost + heuristicCost;
        }
        private int GetHeuristicCost(Node from, Node to)
        {
            // Assuming orthogonal movement cost is 10 and diagonal movement cost is 14 (approximation of sqrt(2) * 10)
            int xDistance = Mathf.Abs(from.position.x - to.position.x);
            int yDistance = Mathf.Abs(from.position.y - to.position.y);
            int diagonalMoves = Mathf.Min(xDistance, yDistance);
            int straightMoves = Mathf.Abs(xDistance - yDistance);

            return (diagonalMoves * 14) + (straightMoves * 10);
        }

    }
}

