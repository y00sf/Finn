using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [Header("Node Identity")]
    public string id = "new_node";

    [Header("Content")]
    public string speakerName = "NPC"; 
    [TextArea(3, 6)]
    public string text = "";

    [Header("Navigation")]
    public string nextNodeID = ""; 

    [Header("Flag Operations (Optional)")]
    public bool setFlag = false;
    public string flagToSet = "";
    public bool flagValue = false;
    
    [Header("Branching Condition (Optional)")]
    public bool useCondition = false;
    public string requiredFlag = "";
    public string trueNodeID = "";
    public string falseNodeID = "";
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(text);
    }
}