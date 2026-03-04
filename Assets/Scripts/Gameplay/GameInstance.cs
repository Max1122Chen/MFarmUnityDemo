using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class GameInstance : Singleton<GameInstance>
{
    [Header("Game Settings")]
    [SerializeField] public GameSettings gameSettings = new GameSettings();
    
    private GameObject MainCanvasObject;
    

    public void Start()
    {

        // Find GO with tag "MainCanvas"
        MainCanvasObject = GameObject.FindGameObjectWithTag("MainCanvas");
        
        if(MainCanvasObject == null)
        {
            Debug.LogError("MainCanvas GameObject with tag 'MainCanvas' not found in the scene.");
        }
    }

    public GameObject SpawnGameObjectInWorld(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        var GO = Instantiate(prefab, spawnPosition, spawnRotation);

        return GO;
    }

    public GameObject CreateUI(GameObject uiPrefab, Transform parent = null)
    {
        var uiGO = Instantiate(uiPrefab, Vector3.zero, Quaternion.identity);
        if (parent == null)
        {
            uiGO.transform.SetParent(MainCanvasObject.transform, false);
        }
        else
        {
            uiGO.transform.SetParent(parent, false);
        }
        return uiGO;
    }
}
