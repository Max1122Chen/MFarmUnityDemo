using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talkable : Interactable
{
    NPCController npcController;

    [Header("Interaction Settings")]
    public float interactionRange = 2f;  // The maximum distance at which the player can interact with the NPC. This can be set in the inspector for each Talkable NPC.

    public void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

    public override bool Interact(GameObject interactor, int mouseButton)
    {
        if(mouseButton != 1)
        {
            return false;
        }

        if (!CheckIfPlayerInInteractionRange(interactor, interactionRange))
        {
            Debug.Log("Player is too far away to interact with " + npcController.NPCName);
            return false;
        }
        else
        {
            PlayerController playerController = interactor.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Communicate(playerController);
            }
        }
        return true;
    }

    private void Communicate(PlayerController playerController)
    {
        Debug.Log("Communicating with " + npcController.NPCName);

        Vector2 playerNPCrelativePosition = playerController.transform.position - transform.position;
        // We can use the relative position to determine which portrait to show for the NPC, and which portrait to show for the player.

        foreach(DialogSnippet dialogSnippet in npcController.NPCData.dialogSnippets)
        {
            if(dialogSnippet.requiredNPCState == npcController.currentState)
            {
                // Show the dialog snippet in the dialog box UI
                CharacterPortrait npcPortrait = npcController.NPCData.portraits.Find(p => p.portraitType == dialogSnippet.portraitType);
                CharacterPortrait playerPortrait = playerController.portraits.Find(p => p.portraitType == dialogSnippet.portraitType);
                Sprite npcPortraitSprite = null;
                Sprite playerPortraitSprite = null;

                if (npcPortrait != null)
                {
                    npcPortraitSprite = npcPortrait.portraitSprite;
                }
                else
                {
                    Debug.LogWarning("No portrait found for NPC " + npcController.NPCName + " with portrait type " + dialogSnippet.portraitType);
                }

                if (playerPortrait != null)
                {
                    playerPortraitSprite = playerPortrait.portraitSprite;
                }
                else
                {
                    Debug.LogWarning("No portrait found for Player with portrait type " + dialogSnippet.portraitType);
                }

                    
                if(playerNPCrelativePosition.x > 0)
                {
                    // Show the player's portrait on the left and the NPC's portrait on the right
                    NPCSubsystem.Instance.dialogBoxUI.ShowDialog(playerPortraitSprite, npcPortraitSprite, dialogSnippet.dialogText);

                }
                else
                {
                    // Show the NPC's portrait on the left and the player's portrait on the right
                    NPCSubsystem.Instance.dialogBoxUI.ShowDialog(npcPortraitSprite, playerPortraitSprite, dialogSnippet.dialogText);
                }
                    
                    
                break;  // We only show the first dialog snippet that matches the NPC's current state. We can expand this later to show multiple snippets or to have more complex dialog trees.
            }
        }
    }
}
