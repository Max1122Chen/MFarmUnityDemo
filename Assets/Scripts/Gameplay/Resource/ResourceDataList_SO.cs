using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceDataList_SO", menuName = "Resource/ResourceDataList_SO")]
public class ResourceDataList_SO : ScriptableObject
{
    public List<ResourceDefinition> resourceDataList;
}
