using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScenePortal : MonoBehaviour
{
    [SceneName][SerializeField] private string targetSceneName;
    [SerializeField] private Vector2 characterSpawnPosition;

    protected void teleport(GameObject character)
    {
        if(character.CompareTag("Player"))
        {
            // Start coroutine by using GameMapSubsystem's instance, otherwise if we start coroutine by using this ScenePortal's instance, the coroutine will be stopped when this ScenePortal is destroyed during scene transition, and the scene transition will not be completed.
            GameMapSubsystem.Instance.StartCoroutine(GameMapSubsystem.Instance.SwitchScene(targetSceneName));
            character.transform.position = characterSpawnPosition;
        }
        else
        {
            // TODO: Teleport other characters (e.g. NPCs, monsters) if needed in the future.
        }


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        teleport(collision.gameObject);
    }
}
