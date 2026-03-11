using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    


public class CharacterAnimationBlueprint : MonoBehaviour
{
    protected Animator[] animators;
    

    // Should be in PlayerABP, for some reason it stays here for now.
    [SerializeField] protected List<PlayerAnimationData> animationDataList = new List<PlayerAnimationData>();

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

    
}