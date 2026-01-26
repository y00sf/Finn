using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Settings")]
    public string dialogueName = "New Dialogue";
    public NPC npc;

    [Header("Dialogue Lines")]
    public List<DialogueNode> lines = new List<DialogueNode>();


    public DialogueNode GetNode(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            Debug.LogError($"[DialogueData] Attempted to get node with empty ID in '{dialogueName}'");
            return null;
        }

        foreach (var node in lines)
        {
            if (node.id == nodeID)
            {
                return node;
            }
        }

        Debug.LogError($"[DialogueData] Node '{nodeID}' not found in '{dialogueName}'");
        return null;
    }


    public DialogueNode GetNodeAtIndex(int index)
    {
        if (index < 0 || index >= lines.Count)
        {
            return null;
        }
        return lines[index];
    }

 
    public bool Validate()
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogError($"[DialogueData] '{dialogueName}' has no lines!");
            return false;
        }
        
        HashSet<string> ids = new HashSet<string>();
        foreach (var node in lines)
        {
            if (!string.IsNullOrEmpty(node.id))
            {
                if (ids.Contains(node.id))
                {
                    Debug.LogError($"[DialogueData] Duplicate node ID '{node.id}' in '{dialogueName}'");
                    return false;
                }
                ids.Add(node.id);
            }
        }

   
        foreach (var node in lines)
        {
            if (!node.IsValid())
            {
                Debug.LogWarning($"[DialogueData] Node '{node.id}' is invalid in '{dialogueName}'");
            }
        }

        return true;
    }

    private void OnValidate()
    {
        if (lines != null && lines.Count > 0)
        {
            Validate();
        }
    }
}