using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceABP : MonoBehaviour
{
    private Animator animator;
    private Resource resourceComponent;

    private const string ANIM_PARAM_IS_BEING_GATHERED = "IsBeingGathered";

    private const string ANIM_PARAM_FINISHED_GATHERING = "FinishedGathering";
    private const string ANIM_PARAM_GATHERING_DIRECTION = "GatheringDirection";

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        resourceComponent = GetComponent<Resource>();
    }

    void Start()
    {
        resourceComponent.onBeingGathered += HandleBeingGathered;
        resourceComponent.onFinishedGathering += HandleFinishedGathering;
    }

    void OnDestroy()
    {
        resourceComponent.onBeingGathered -= HandleBeingGathered;
        resourceComponent.onFinishedGathering -= HandleFinishedGathering;
    }
    void HandleBeingGathered(GameObject gatherer)
    {
        CalculateAnimationDirection(gatherer.transform.position);

        animator.SetTrigger(ANIM_PARAM_IS_BEING_GATHERED);
    }

    void HandleFinishedGathering(GameObject gatherer)
    {
        CalculateAnimationDirection(gatherer.transform.position);

        animator.SetTrigger(ANIM_PARAM_FINISHED_GATHERING);
    }

    void CalculateAnimationDirection(Vector3 gathererPosition)
    {
        float direction = gathererPosition.x - transform.position.x;
        direction = Mathf.Sign(direction); // -1 for left, 1 for right
        
        animator.SetFloat(ANIM_PARAM_GATHERING_DIRECTION, direction);
    }
}
