using System.Collections;
using System.Collections.Generic;
using TimeSystem;
using UnityEngine;

[System.Serializable]
public enum NPCState
{
    Idle = 0,
    Walking = 1,
    Working = 2,
}

// Defines the schedule for an NPC, including where they should be at different times and seasons. This will be used by the NPCController to move the NPC around the map according to their schedule.
[System.Serializable]
public class NPCScheduleDefinition
{
    public string scheduleName;  // A name for this schedule, which can be used for debugging and reference purposes.
    public NPCState state;  // The state the NPC should be in during this schedule (e.g. Idle, Walking, Working). This can be used to determine the NPC's behavior and animations during this schedule.

    [Header("Traveling Settings")]
    [SceneName] public string targetScene;
    public Vector2 targetPosition;

    [Header("Schedule Timing Settings")]
    public GameSeason activeSeason;
    
    // public int startMonth, endMonth;    // Since we have seasons, we can just use season to determine when the schedule is active. We can add month range later if needed.
    public int startDay, endDay;    // The range of days in the month when this schedule is active. 0 means every day.
    public int startHour, startMinute;
    public int durationInMinutes;  // The duration of this schedule in minutes. After this duration, the NPC can switch to the next schedule if there is one available. This can be used to create schedules that only last for a certain amount of time, even if the time conditions are still met.
}

[System.Serializable]
public enum CharacterPortraitType
{
    Normal = 0,
    Happy = 1,
    Sad = 2,
    Angry = 3,
    Surprised = 4,
    // Add more portrait types as needed
}

[System.Serializable]
public class CharacterPortrait
{
    public CharacterPortraitType portraitType;
    public Sprite portraitSprite;
}

[System.Serializable]
public class NPCData
{
    public string npcName;
    public List<CharacterPortrait> portraits = new List<CharacterPortrait>();  // A list of different portraits for the NPC, which can be used in different situations (e.g. different expressions for dialogue).

    // The prefab to spawn for this NPC, it should have the NPCController component on it.
    // It only serves as a "model" for the NPC, without any specific data assigned to it. When we spawn the NPC in the world, we will create a new instance of this prefab and then initialize it with the specific NPCData.
    public GameObject npcPrefab;  
    [SceneName] public string initialScene;
    public Vector2 initialPosition;

    public List<NPCScheduleDefinition> scheduleList = new List<NPCScheduleDefinition>();
    public List<DialogSnippet> dialogSnippets = new List<DialogSnippet>();  // A list of dialog snippets for the NPC, which can be used in different situations (e.g. different dialog for different quests or events).
}

[CreateAssetMenu(fileName = "NPCDataList_SO", menuName = "NPCDataList_SO")]
public class NPCDataList_SO : ScriptableObject
{
    public List<NPCData> npcDataList = new List<NPCData>();
}
