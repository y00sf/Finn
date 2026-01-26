using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public string conversationID;
    public DialogueData dialogue;
}

public class DialogueNPC : MonoBehaviour
{
    [Header("Conversation Settings")]
    public string defaultConversationID = "greeting";

    [Header("Conversation Registry")]
    public List<DialogueEntry> allConversations = new List<DialogueEntry>();

    [SerializeField] private NPC npc;

    [Header("Optional Settings")]
    public bool useReturnConversation = false;
    public string returnConversationID = "return_greeting";
    
    private string flagKey;
    private DialogueUI cachedUI;

    private void Start()
    {
        flagKey = $"met_{gameObject.name}";
        cachedUI = FindObjectOfType<DialogueUI>();
    }
    
    public void Interact()
    {
        string conversationToPlay = defaultConversationID;

        if (useReturnConversation)
        {
            bool hasMet = GameFlags.Instance.GetFlag(flagKey);
            conversationToPlay = hasMet ? returnConversationID : defaultConversationID;
            
            if (!hasMet)
            {
                GameFlags.Instance.SetFlag(flagKey, true);
            }
        }

        PlayConversation(conversationToPlay);
    }
    
    public void PlayConversation(string id)
    {
        DialogueData dialogueToPlay = GetDialogueByID(id);
        
        if (dialogueToPlay != null)
        {
            if (ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartDialogue(dialogueToPlay, npc ,transform);
            }
        }
        else
        {
            Debug.LogWarning($"NPC '{gameObject.name}' could not find conversation ID: {id}");
        }
    
    }
    
    public void SetActiveConversation(string newID)
    {
        defaultConversationID = newID;
        if (GetDialogueByID(newID) == null)
        {
            Debug.LogWarning($"[DialogueNPC] Warning: Switching to conversation ID '{newID}' but it was not found in the list.");
        }
        else
        {
            Debug.Log($"NPC '{gameObject.name}' default conversation changed to: {newID}");
        }
    }

    private DialogueData GetDialogueByID(string id)
    {
        foreach (var entry in allConversations)
        {
            if (entry.conversationID == id) return entry.dialogue;
        }
        return null;
    }
}