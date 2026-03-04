using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreeDefinition
{
    public int treeID = -1;     // if the tree is related to an item, treeID should be the same as the itemID of the related item
    public Sprite crownSprite;
    public Sprite stumpSprite;
}

[CreateAssetMenu(fileName = "TreeDataList_SO", menuName = "TreeDataList_SO")]
public class TreeDataList_SO : ScriptableObject
{
    public List<TreeDefinition> treeDataList = new List<TreeDefinition>();
}
