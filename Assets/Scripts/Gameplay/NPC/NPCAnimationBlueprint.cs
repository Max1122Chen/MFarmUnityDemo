using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimationBlueprint : CharacterAnimationBlueprint
{
    private NPCController npcController;
    const string ANIM_PRAM_MOVE_SPEED = "MoveSpeed";
    const string ANIM_PRAM_INPUT_X = "InputX";
    const string ANIM_PRAM_INPUT_Y = "InputY";
    public override void Awake()
    {
        base.Awake();
        npcController = GetComponent<NPCController>();
        npcController.onLateUpdate += UpdateAnimations;
    }

    public override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        foreach(Animator animator in animators)
        {
            animator.SetFloat(ANIM_PRAM_MOVE_SPEED, npcController.movementInput.magnitude);
            animator.SetFloat(ANIM_PRAM_INPUT_X, npcController.movementInput.x);
            animator.SetFloat(ANIM_PRAM_INPUT_Y, npcController.movementInput.y);
        }
    }
}
