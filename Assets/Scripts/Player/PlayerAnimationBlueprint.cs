using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using InventorySystem;

public class PlayerAnimationBlueprint : CharacterAnimationBlueprint
{
    private PlayerController pc;

    [SerializeField] private SpriteRenderer heldItemSpriteRenderer;

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
            SwitchAOC(CharacterAnimationAction.Hold);
        }
        else
        {
            heldItemSpriteRenderer.enabled = false;
            SwitchAOC(CharacterAnimationAction.Normal);
        }
    }

    private void HandleUseTool(ItemDefinition itemDef)
    {
        if(itemDef != null && itemDef.IsValidItem())
        {
            switch(itemDef.itemType)
            {
                case ItemType.Hoe:
                    SwitchAOC(CharacterAnimationAction.Hoe);
                    break;
                case ItemType.Axe:
                    SwitchAOC(CharacterAnimationAction.Axe);
                    break;
                case ItemType.Pickaxe:
                    SwitchAOC(CharacterAnimationAction.Pickaxe);
                    break;
                case ItemType.WateringCan:
                    SwitchAOC(CharacterAnimationAction.WateringCan);
                    break;
                default:
                    SwitchAOC(CharacterAnimationAction.Normal);
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
}
