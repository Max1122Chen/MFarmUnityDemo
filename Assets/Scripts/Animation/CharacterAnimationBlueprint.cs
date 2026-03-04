using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CharacterAnimationPart
{
    None,
    Body,
    Arm,
    Hair,
    Tool

}

public enum CharacterAnimationAction
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
public struct CharacterAnimationData
{
    public CharacterAnimationPart animationPart;
    public CharacterAnimationAction animationAction;
    public AnimatorOverrideController aoc;
}
    


public class CharacterAnimationBlueprint : MonoBehaviour
{
    protected Animator[] animators;
    protected Dictionary<string, Animator> animatorDict = new Dictionary<string, Animator>();


    [SerializeField] protected List<CharacterAnimationData> animationDataList = new List<CharacterAnimationData>();

    public virtual void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
    }

    public virtual void Start()
    {
    }

    protected virtual void InitializeAnimation()
    {
    }

    public virtual void UpdateAnimations()
    {
    }

    public virtual void SwitchAOC(CharacterAnimationAction animationAction)
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