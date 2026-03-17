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
    public NPCData NPCData => npcData;  // Expose NPCData as a public read-only property, so other classes can access the NPC's data without being able to modify it.
    public NPCState currentState = NPCState.Idle;
    public string NPCName => npcData != null ? npcData.npcName : "Unknown NPC";

    // NPC Schedule
    HashSet<int> completedScheduleIndices = new HashSet<int>();  // To track which schedules have been completed for the current day, so we don't repeat the same schedule multiple times in the same day.
    bool isExecutingSchedule = false;  // To track whether the NPC is currently executing a schedule, so we don't interrupt it with another schedule until it's done.
    int currentScheduleIndex = -1;  // To track the current active schedule for the NPC, -1 means no active schedule.

    // Movement Parameters
    [SerializeField] private float moveSpeed = 4f;
    private bool isMoving = false;
    public Vector2 movementInput;



    // Scene and Position Tracking
    private string currentScene;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        npcABP = GetComponentInChildren<NPCAnimationBlueprint>();
        navigationAgent = GetComponent<AStarNavigationAgent>();

        TimeSubsystem.Instance.onDayPassed += (currentDay) => completedScheduleIndices.Clear();  // Clear the completed schedule indices at the end of each day to allow schedules to be executed again the next day.
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

        EvaluateSchedules();
    }

    private void EvaluateSchedules()
    {
        GameTime currentGameTime = TimeSubsystem.Instance.GetCurrentGameTime();

        for(int i = 0; i < npcData.scheduleList.Count; i++)
        {
            if(i == currentScheduleIndex)
            {
                // If the current schedule is still active, we don't need to check the rest of the schedules.
                // Since the npc should only do one thing at a time
                if(isExecutingSchedule)
                {
                    break;
                }
                else
                {
                    // If the schedule is no longer active, we can mark it as completed and check for the next schedule.
                    completedScheduleIndices.Add(currentScheduleIndex);
                    currentScheduleIndex = -1;  // Reset current schedule index to allow checking for the next schedule in the next update cycle.
                }
            }
            var schedule = npcData.scheduleList[i];


            if(schedule.activeSeason == currentGameTime.season &&
               (schedule.startDay == 0 || (currentGameTime.day >= schedule.startDay && currentGameTime.day <= schedule.endDay)) &&
               (currentGameTime.hour > schedule.startHour || (currentGameTime.hour == schedule.startHour && currentGameTime.minute >= schedule.startMinute)))
            {
                isExecutingSchedule = true;
                currentScheduleIndex = i;

                switch(schedule.state)
                {
                    case NPCState.Idle:
                        currentState = NPCState.Idle;
                        // Do nothing for now
                        break;
                    case NPCState.Walking:
                        currentState = NPCState.Walking;
                        MoveAsScheduled(schedule);
                        break;
                    case NPCState.Working:
                        currentState = NPCState.Working;
                        // TODO: Implement working behavior, which could include playing working animation and maybe preventing movement for a certain duration to simulate working.
                        WorkAsScheduled();
                        break;
                }

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


    // NPC behavior methods based on schedules
    // Traveling behavior
    public void MoveAsScheduled(NPCScheduleDefinition schedule)
    {
        // Check if we are already in the target scene and close enough to the target position, if so, we don't need to move.
        if(currentScene == schedule.targetScene && Vector2.Distance(transform.position, schedule.targetPosition) < 0.5f)
        {
            return;
        }


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

        isExecutingSchedule = false;  // Mark the schedule as completed after reaching the target position.
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

    // Working behavior
    public void WorkAsScheduled()
    {
        // For now, we will just simulate working by waiting for the duration of the schedule, and we can play working animation in the meantime. We will also prevent the NPC from doing any other actions during this time to simulate working.
        StartCoroutine(WorkAsScheduledCoroutine());
    }

    private IEnumerator WorkAsScheduledCoroutine()
    {
        int lastGameMinute = TimeSubsystem.Instance.GetCurrentGameTime().minute;
        int timeAccumulated = 0;
        while (timeAccumulated < npcData.scheduleList[currentScheduleIndex].durationInMinutes)
        {
            timeAccumulated += TimeSubsystem.Instance.GetCurrentGameTime().minute - lastGameMinute;
            lastGameMinute = TimeSubsystem.Instance.GetCurrentGameTime().minute;
            yield return null;
        }
        isExecutingSchedule = false;  // Mark the schedule as completed after working for the specified duration.
        yield break;
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
