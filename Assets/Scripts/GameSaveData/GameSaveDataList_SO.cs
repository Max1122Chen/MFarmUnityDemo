using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// It seems that SO is not a good way to save game data for release version, because SO is not designed to be used as a data container for runtime data, and it will cause some issues when we try to save game data to it, such as the data will be lost when we exit the game, and it will also cause some issues when we try to load game data from it, such as the data will be reset to the default value when we load the game, etc.
// For now we will just use SO to store the game save data for all save files, and we will implement the save/load system later to allow player to choose which save file to load, and we will also implement the save/load system later to save game data to a file instead of SO for release version, so that we can avoid the issues caused by using SO to store game save data for runtime data.
[CreateAssetMenu(fileName = "GameSaveDataList_SO", menuName = "SaveData/GameSaveDataList_SO")]
public class GameSaveDataList_SO : ScriptableObject
{
    public List<GameSaveData> gameSaveDataList = new List<GameSaveData>();
}

