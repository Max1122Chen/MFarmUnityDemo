using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogSnippet
{
    public NPCState requiredNPCState;  // The required state of the NPC for this dialog snippet to be active. This can be used to create different dialog options based on the NPC's current behavior (e.g. different dialog when the NPC is working vs when they are idle).
    public CharacterPortraitType portraitType;
    public string dialogText;
}
