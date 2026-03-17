using System.Collections;
using System.Collections.Generic;
using Navigation.AStar;
using UnityEngine;
using TimeSystem;
using UnityEngine.Tilemaps;
using System;


public class NPCController : MonoBehaviour
{
    // Component References
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private NPCAnimationBlueprint npcABP;
    private AStarNavigationAgent navigationAgent;

    public Action onLateUpdate;

    // NPC Data Reference
    private NPCData npcData;
    public string NPCName => npcData != null ? npcData.npcName : "Unknown NPC";

    // Movement Parameters
    [SerializeField] private float moveSpeed = 4f;
    private bool isMoving = false;
    public Vector2 movementInput;

    int currentScheduleIndex = -1;  // To track the current active schedule for the NPC, -1 means no active schedule.

    // Scene and Position Tracking
    private string currentScene;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        npcABP = GetComponentInChildren<NPCAnimationBlueprint>();
        navigationAgent = GetComponent<AStarNavigationAgent>();

        GameMapSubsystem.Instance.onNewSceneLoaded += HandleNewSceneLoaded;
    }

    public void Initialize(NPCSaveData saveData)
    {
        // Load the NPC data from the save data. This will be called when we spawn the NPC in the world, and we will pass in the specific NPCSaveData for this NPC.
        npcData = NPCSubsystem.Instance.GetNPCDataByName(saveData.npcName);
        currentScene = saveData.currentScene;
        transform.position = saveData.position;
    }

    public void Update()
    {

        isMoving = rb.velocity.magnitude > 0.1f;

        GameTime currentGameTime = TimeSubsystem.Instance.GetCurrentGameTime();
        for(int i = 0; i < npcData.scheduleList.Count; i++)
        {
            if(i == currentScheduleIndex)
            {
                // If the current schedule is still active, we don't need to check the rest of the schedules.
                break;
            }
            var schedule = npcData.scheduleList[i];

            if(schedule.activeSeason == currentGameTime.season &&
               (schedule.startDay == 0 || (currentGameTime.day >= schedule.startDay && currentGameTime.day <= schedule.endDay)) &&
               (currentGameTime.hour > schedule.startHour || (currentGameTime.hour == schedule.startHour && currentGameTime.minute >= schedule.startMinute)))
            {
                currentScheduleIndex = i;
                MoveAsScheduled(schedule);
                break;  // We can break here since we assume the schedules are non-overlapping,
            }
        }
    }


    public void LateUpdate()
    {
        onLateUpdate.Invoke();
    }

    public void SetActiveInWorld(bool isActive)
    {
        spriteRenderer.enabled = isActive;
        npcABP.enabled = isActive;
    }

    public void Movement(Vector2 direction)
    {
        movementInput = direction * moveSpeed;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    public void MoveAsScheduled(NPCScheduleDefinition schedule)
    {
        List<BridgeToScene> bridgeRoute = PlanARouteToTarget(schedule.targetScene, schedule.targetPosition);
        if (bridgeRoute != null)
        {
            StartCoroutine(MoveAlongRouteCoroutine(bridgeRoute, schedule.targetPosition));
        }
        else
        {
            Debug.LogWarning($"NPCController: No route found for NPC {NPCName} to move from scene {currentScene} to target scene {schedule.targetScene} and position {schedule.targetPosition}.");
        }
    }

    public IEnumerator MoveAlongRouteCoroutine(List<BridgeToScene> bridgeRoute, Vector2 targetPosition)
    {
        Debug.Log($"NPCController: Starting to move along route to target position {targetPosition} in scene {currentScene}.");

        TileInfo currentTileInfo = null;
        TileInfo targetTileInfo = null;

        Vector2Int currentGridPos = Vector2Int.zero;
        Vector2Int targetGridPos = Vector2Int.zero;

        // First move through the bridges to transition between scenes until we reach the target scene, then move towards the target position in the final scene.
        foreach (var bridge in bridgeRoute)
        {
            // Reset variable for the new leg of journey
            currentTileInfo = null;
            targetTileInfo = null;

            // Move towards the bridge position in the current scene
            while(currentTileInfo == null || targetTileInfo == null)
            {
                currentTileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(transform.position, currentScene);
                targetTileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(bridge.fromPos, currentScene); // Pay attention here, we want to get the tile info of the bridge position in the current scene, since that's where we want to go towards in the current scene before we can transition to the target scene.
                yield return null;  // Wait until we can get valid tile info for both current position and target position
            }
            currentGridPos = currentTileInfo.gridPos;
            targetGridPos = targetTileInfo.gridPos;

            List<Node> pathToBridge = navigationAgent.FindPath(currentGridPos, targetGridPos, currentScene);
            if (pathToBridge != null)
            {
                foreach (var node in pathToBridge)
                {
                    Vector2 targetWorldPos = GameMapSubsystem.Instance.GetTileInfoByGridPos(node.position, currentScene).worldPos;
                    while (Vector2.Distance(transform.position, targetWorldPos) > 0.1f)
                    {
                        Vector2 direction = (targetWorldPos - (Vector2)transform.position).normalized;
                        Movement(direction);
                        yield return null;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"NPCController: No path found for NPC {NPCName} to move from position {transform.position} to bridge position {bridge.fromPos} in scene {currentScene}.");
                yield break;  // If we can't even reach the bridge, we can't continue with the route, so we stop the coroutine.
            }
            
            // Simulate scene transition by updating currentScene and teleporting to the new scene's entry point
            this.transform.position = bridge.toPos;
            currentScene = bridge.toScene;

            if(GameMapSubsystem.Instance.currentSceneName != currentScene)
            {
                SetActiveInWorld(false);
            }
        }

        // Reset variable for the final leg
        currentTileInfo = null;
        targetTileInfo = null;

        // After reaching the final scene, move towards the target position
        while(currentTileInfo == null || targetTileInfo == null)
        {
            currentTileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(transform.position, currentScene);
            targetTileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(targetPosition, currentScene);
            yield return null;  // Wait until we can get valid tile info for both current position and target position
        }
        currentGridPos = currentTileInfo.gridPos;
        targetGridPos = targetTileInfo.gridPos;

        List<Node> pathToTarget = navigationAgent.FindPath(currentGridPos, targetGridPos, currentScene);
        if (pathToTarget != null)
        {
            foreach (var node in pathToTarget)
            {
                Vector2 targetWorldPos = GameMapSubsystem.Instance.GetTileInfoByGridPos(node.position, currentScene).worldPos;
                while (Vector2.Distance(transform.position, targetWorldPos) > 0.1f)
                {
                    Vector2 direction = (targetWorldPos - (Vector2)transform.position).normalized;
                    Movement(direction);
                    yield return null;
                }
            }
        }
        else
        {
            Debug.LogWarning($"NPCController: No path found for NPC {NPCName} to move from position {transform.position} to target position {targetPosition} in scene {currentScene}.");
            yield break;  // If we can't reach the target position, we stop the coroutine.
        }

        currentScheduleIndex = -1;  // After reaching the target position, we can reset the current schedule index to allow checking for the next schedule in the next update cycle.
    }

    private List<BridgeToScene> PlanARouteToTarget(string targetScene, Vector2 targetPosition)
    {
        List<string> route = FindARouteBetweenScenes(currentScene, targetScene);
        if (route == null)        
        {
            Debug.LogWarning($"NPCController: No route found from scene {currentScene} to scene {targetScene}.");
            return null;
        }

        List<BridgeToScene> bridgeRoute = new List<BridgeToScene>();
        string sceneCache = currentScene;
        foreach (var scene in route)
        {
            Debug.Log($"NPCController: Route step - {scene}");
            BridgeToScene bridge = GameMapSubsystem.Instance.sceneAdjacencyDict[sceneCache].bridgesToOtherScenes.Find(b => b.toScene == scene);
            bridgeRoute.Add(bridge);
            sceneCache = scene;
        }
        
        return bridgeRoute;
    }
    
    private List<string> FindARouteBetweenScenes(string startScene, string targetScene)
    {
        HashSet<string> visited = new HashSet<string>();
        Stack<string> stack = new Stack<string>();
        Dictionary<string, string> parentMap = new Dictionary<string, string>();

        stack.Push(startScene);
        visited.Add(startScene);

        while (stack.Count > 0)
        {
            string currentScene = stack.Pop();
            if (currentScene == targetScene)
            {
                // Backtrack to find the route
                List<string> route = new List<string>();
                while (currentScene != startScene)
                {
                    route.Add(currentScene);
                    currentScene = parentMap[currentScene];
                }
                // route.Add(startScene);   // We don't need to add the start scene to the route, since the NPC is already in the start scene.
                route.Reverse();
                return route;
            }

            if(GameMapSubsystem.Instance.sceneAdjacencyDict.ContainsKey(currentScene))
            {                
                SceneAdjacency adjacency = GameMapSubsystem.Instance.sceneAdjacencyDict[currentScene]; 
                foreach (var bridge in adjacency.bridgesToOtherScenes)
                {
                    string adjacentScene = bridge.toScene;
                    if (!visited.Contains(adjacentScene))
                    {
                        visited.Add(adjacentScene);
                        stack.Push(adjacentScene);
                        parentMap[adjacentScene] = currentScene;
                    }
                }
            }
            
        }

        return null; // No route found
    }

    private void HandleNewSceneLoaded(string newSceneName)
    {
        if (currentScene == newSceneName)
        {
            SetActiveInWorld(true);
        }
        else
        {
            SetActiveInWorld(false);
        }
    }
}
