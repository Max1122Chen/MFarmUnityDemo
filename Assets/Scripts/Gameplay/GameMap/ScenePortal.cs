using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScenePortal : Interactable
{
    [SceneName][SerializeField] private string targetSceneName;
    public bool needInteraction = false; // If true, player needs to interact (e.g. press a key) to trigger the teleportation. If false, teleportation will be triggered immediately upon entering the portal.
    public bool keepX = false;
    public bool keepY = false;
    [SerializeField] private Vector2 characterSpawnPosition;

    protected void teleport(GameObject character)
    {
        if(character.CompareTag("Player"))
        {
            Vector2 finalSpawnPosition = characterSpawnPosition;
            if(keepX)
            {
                finalSpawnPosition.x = character.transform.position.x;
            }
            if(keepY)
            {
                finalSpawnPosition.y = character.transform.position.y;
            }
            GameMapSubsystem.Instance.StartCoroutine(GameMapSubsystem.Instance.TeleportPlayerToScene(targetSceneName, character, finalSpawnPosition));
        }
        else
        {
            // TODO: Teleport other characters (e.g. NPCs, monsters) if needed in the future.
        }


    }

    public override bool Interact(GameObject interactor, int mouseButton)
    {
        if(mouseButton != 1) // Only respond to right-click interactions
        {
            return false;
        }

        if(!CheckIfPlayerInInteractionRange(interactor))
        {
            return false;
        }

        teleport(interactor);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!needInteraction)
        {
            teleport(collision.gameObject);
        }
    }
}
