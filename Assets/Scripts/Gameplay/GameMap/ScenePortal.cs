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
            GameMapSubsystem.Instance.StartCoroutine(GameMapSubsystem.Instance.TeleportPlayerToScene(targetSceneName, character, characterSpawnPosition));
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
