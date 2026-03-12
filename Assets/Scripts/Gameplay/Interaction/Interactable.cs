using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is just an interface for anything that can be interacted with (e.g. container, vendor, NPC, etc.)
public class Interactable : MonoBehaviour
{
    // Return true if interaction is successful, false if interaction failed (e.g. player is too far to interact)
    public virtual bool Interact(GameObject interactor, int mouseButton)
    {
        return true;
    }

}
