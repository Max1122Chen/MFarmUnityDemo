using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField] private SpriteRenderer crownSprite;
    [SerializeField] private SpriteRenderer stumpSprite;

    private void Awake()
    {
        
    }

    private void Start()
    {
        Resource resource = GetComponent<Resource>();
        TreeDefinition treeDef = ResourceSubsystem.Instance.GetTreeDefinition(resource.resourceID);

        crownSprite.sprite = treeDef.crownSprite;
        stumpSprite.sprite = treeDef.stumpSprite;
    }

}
