using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InteractionResult
{
    public bool foundInteractable;
    public bool succeededInInteraction;
    public string failureReason; 

}
public class Interactable : MonoBehaviour
{
    [SerializeField] protected float interactionRadius  = -1f;  // -1 measns interactable from any distance or does not require this to distance check for interaction

    public virtual void Awake()
    {
    }

    // Return true if interaction is successful, false if interaction failed (e.g. player is too far to interact)
    public virtual bool Interact(GameObject interactor, int mouseButton)
    {
        if(interactionRadius  < 0)
        {
            return true;
        }

        if(Vector2.Distance(interactor.transform.position, transform.position) <= interactionRadius)
        {
            #if UNITY_EDITOR
            Debug.Log($"{interactor.name} interacted with {gameObject.name}");
            Debug.DrawLine(interactor.transform.position, (transform.position - interactor.transform.position).normalized * interactionRadius + interactor.transform.position, Color.green, 1f);
            #endif
        }
        else
        {
            #if UNITY_EDITOR
            Debug.Log($"{interactor.name} is too far to interact with {gameObject.name}");
            Debug.DrawLine(interactor.transform.position, (transform.position - interactor.transform.position).normalized * interactionRadius + interactor.transform.position, Color.red, 1f);
            #endif

            return false;
        }
        return true;
    }

}
