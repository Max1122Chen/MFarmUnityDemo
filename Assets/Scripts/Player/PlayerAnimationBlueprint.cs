using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using InventorySystem;

[System.Serializable]
public enum PlayerAnimationPart
{
    None,
    Body,
    Arm,
    Hair,
    Tool

}

public enum PlayerAnimationAction
{
    Normal,
    Hold,
    Hoe,
    Axe,
    Pickaxe,
    WateringCan,
    Harvest
}

[System.Serializable]
public struct PlayerAnimationData
{
    public PlayerAnimationPart animationPart;
    public PlayerAnimationAction animationAction;
    public AnimatorOverrideController aoc;
}


public class PlayerAnimationBlueprint : CharacterAnimationBlueprint
{
    private PlayerController pc;

    [SerializeField] private SpriteRenderer heldItemSpriteRenderer;

    private Dictionary<string, Animator> animatorDict = new Dictionary<string, Animator>();

    protected const string ANIM_PARAM_INPUT_X = "InputX";
    protected const string ANIM_PARAM_INPUT_Y = "InputY";
    protected const string ANIM_PARAM_MOVE_SPEED = "MoveSpeed";

    protected const string ANIM_PARAM_MOUSE_X = "MouseX";
    protected const string ANIM_PARAM_MOUSE_Y = "MouseY";
    protected const string ANIM_PARAM_USE_TOOL = "UseTool";

    public override void Awake()
    {
        base.Awake();

        foreach(var animator in animators)
        {
            if(!animatorDict.ContainsKey(animator.gameObject.name))
            {
                animatorDict.Add(animator.gameObject.name, animator);
            }
            else
            {
                Debug.LogWarning($"Duplicate animator name found: {animator.gameObject.name}. Animator names should be unique for easy access.");
            }
        }
    }

    public override void Start()
    {
        base.Start();
        pc = GetComponent<PlayerController>();
        pc.onLateUpdate += UpdateAnimations;
        pc.onHeldItemChanged += HandleHeldItemChanged;
        pc.onUseTool += HandleUseTool;

    }

    protected override void InitializeAnimation()
    {
        base.InitializeAnimation();
    }

    // Update is called once per frame
    public override void UpdateAnimations()
    {
        base.UpdateAnimations();
        foreach(var animator in animators)
        {
            if(pc.movementInput.magnitude > 0)
            {
                animator.SetFloat(ANIM_PARAM_INPUT_X, pc.inputX);
                animator.SetFloat(ANIM_PARAM_INPUT_Y, pc.inputY);
                animator.SetFloat(ANIM_PARAM_MOVE_SPEED, pc.moveSpeed);
            }
            else
            {
                animator.SetFloat(ANIM_PARAM_MOVE_SPEED, 0f);
            }
        }
    }



    // Callbacks

    private void HandleHeldItemChanged(ItemDefinition itemDef)
    {
        if(itemDef != null && itemDef.IsValidItem() && itemDef.isHoldable == true)
        {
            heldItemSpriteRenderer.sprite = itemDef.heldSprite != null ? itemDef.heldSprite : itemDef.itemIcon;   // Use heldSprite if available, otherwise fallback to itemIcon.
            heldItemSpriteRenderer.enabled = true;
            SwitchAOC(PlayerAnimationAction.Hold);
        }
        else
        {
            heldItemSpriteRenderer.enabled = false;
            SwitchAOC(PlayerAnimationAction.Normal);
        }
    }

    private void HandleUseTool(ItemDefinition itemDef)
    {
        if(itemDef != null && itemDef.IsValidItem())
        {
            switch(itemDef.itemType)
            {
                case ItemType.Hoe:
                    SwitchAOC(PlayerAnimationAction.Hoe);
                    break;
                case ItemType.Axe:
                    SwitchAOC(PlayerAnimationAction.Axe);
                    break;
                case ItemType.Pickaxe:
                    SwitchAOC(PlayerAnimationAction.Pickaxe);
                    break;
                case ItemType.WateringCan:
                    SwitchAOC(PlayerAnimationAction.WateringCan);
                    break;
                default:
                    SwitchAOC(PlayerAnimationAction.Normal);
                    break;
            }

            foreach(var animator in animators)
            {
                // Turn the player to face the mouse cursor after using a tool.
                animator.SetFloat(ANIM_PARAM_INPUT_X, pc.mouseX);
                animator.SetFloat(ANIM_PARAM_INPUT_Y, pc.mouseY);

                animator.SetFloat(ANIM_PARAM_MOUSE_X, pc.mouseX);
                animator.SetFloat(ANIM_PARAM_MOUSE_Y, pc.mouseY);
                animator.SetTrigger(ANIM_PARAM_USE_TOOL);
            }
        }
    }

    public void SwitchAOC(PlayerAnimationAction animationAction)
    {
        foreach(var animationData in animationDataList)
        {
            if(animationData.animationAction == animationAction)
            {
                if(animationData.aoc != null)
                {
                    if(animatorDict.TryGetValue(animationData.animationPart.ToString(), out Animator animator))
                    {
                        animator.runtimeAnimatorController = animationData.aoc;
                    }
                }
            }
        }
    }
}
