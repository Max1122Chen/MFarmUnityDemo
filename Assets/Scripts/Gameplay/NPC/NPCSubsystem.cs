using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSubsystem : Singleton<NPCSubsystem>
{
    [Header("NPC Data")]
    // NPCData is read-only data that defines the characteristics of an NPC type, such as name, initial position, dialogue, etc.
    public NPCDataList_SO npcDataList;
    // Key: NPC Name, Value: NPCData
    public Dictionary<string, NPCData> npcDataDict = new Dictionary<string, NPCData>();


    // Runtime NPC Instances
    private List<NPCController> registeredNPCs = new List<NPCController>();
    // Key: NPC Name, Value: NPCController instance in the world. This is used for quick lookup of NPC instances by their name.
    private Dictionary<string, NPCController> npcInstancesDict = new Dictionary<string, NPCController>();

    public void Initialize()
    {
        // Initialize the NPC data dictionary for quick lookup by NPC name
        foreach (NPCData npcData in npcDataList.npcDataList)
        {
            if (!npcDataDict.ContainsKey(npcData.npcName))
            {
                npcDataDict.Add(npcData.npcName, npcData);
            }
        }
    }

    public void RegisterNPC(NPCController npcController)
    {
        if (!registeredNPCs.Contains(npcController))
        {
            registeredNPCs.Add(npcController);
            if (!npcInstancesDict.ContainsKey(npcController.name))
            {
                npcInstancesDict.Add(npcController.name, npcController);
            }
        }
    }

    public void UnregisterNPC(NPCController npcController)
    {
        if (registeredNPCs.Contains(npcController))
        {
            registeredNPCs.Remove(npcController);
            if (npcInstancesDict.ContainsKey(npcController.name))
            {
                npcInstancesDict.Remove(npcController.name);
            }
        }
    }

    public void HandleCreateNewGameSaveData(GameSaveData newGameSaveData)
    {
        // Initialize NPC save data based on the NPC data list in the NPC subsystem, we will update the NPC save data when we save the game, so we need to initialize the data here to avoid reference issue.
        List<NPCSaveData> npcSaveDataList = new List<NPCSaveData>();
        foreach (NPCData npcData in NPCSubsystem.Instance.npcDataList.npcDataList)
        {
            NPCSaveData npcSaveData = new NPCSaveData();
            npcSaveData.npcName = npcData.npcName;
            npcSaveData.currentScene = npcData.initialScene;
            npcSaveData.position = npcData.initialPosition;

            npcSaveDataList.Add(npcSaveData);
        }
        newGameSaveData.npcSaveDataList = npcSaveDataList;
    }

    // TODO: Implement the logic to spawn the NPC in the world based on the npcData, such as instantiating a prefab, setting its position, etc.
    public NPCController SpawnNPCInTheWorld(NPCSaveData npcSaveData, string currentSceneName)
    {
        if (npcDataDict.ContainsKey(npcSaveData.npcName))
        {
            NPCData npcData = npcDataDict[npcSaveData.npcName];
            GameObject npcPrefab = npcData.npcPrefab;
            GameObject newNPC = GameInstance.Instance.SpawnGameObjectInWorld(npcPrefab, npcSaveData.position, Quaternion.identity);
            NPCController npcController = newNPC.GetComponent<NPCController>();
            npcController.Initialize(npcData);
            RegisterNPC(npcController);

            //
            if(npcSaveData.currentScene == currentSceneName)
            {
                npcController.SetActiveInWorld(true);
            }
            else
            {
                npcController.SetActiveInWorld(false);
            }

            return npcController;
        }
        else
        {
            Debug.LogWarning($"NPCSubsystem: NPC with name {npcSaveData.npcName} not found in npcDataDict.");
            return null;
        }
    }

    public NPCData GetNPCDataByName(string npcName)
    {
        if (npcDataDict.ContainsKey(npcName))
        {
            return npcDataDict[npcName];
        }
        else
        {
            Debug.LogWarning($"NPCSubsystem: NPC with name {npcName} not found in npcDataDict.");
            return null;
        }
    }

    // TODO: Implement the logic to save the NPC data, such as current position, current scene, etc. We will use the NPCSaveData class to save the NPC data.
    public List<NPCSaveData> SaveAllNPCs()
    {
        return null;
    }

    public void LoadAllNPCs(List<NPCSaveData> npcSaveDataList, string currentSceneName)
    {
        // We will implement this later when we have the NPC save data to load
        foreach (NPCSaveData npcSaveData in npcSaveDataList)
        {
            SpawnNPCInTheWorld(npcSaveData, currentSceneName);
        }
    }
}
