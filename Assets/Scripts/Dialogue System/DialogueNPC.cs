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

    [Header("Optional Settings")]
    [Tooltip("If true, automatically switches to return conversation after first meeting")]
    public bool useReturnConversation = false;
    public string returnConversationID = "return_greeting";
    private string flagKey;

    private void Start()
    {
        flagKey = $"met_{gameObject.name}";
    }

    
    public void Interact()
    {
        if (useReturnConversation)
        {
            bool hasMet = GameFlags.Instance.GetFlag(flagKey);
            string conversationToPlay = hasMet ? returnConversationID : defaultConversationID;
            PlayConversation(conversationToPlay);
            
            if (!hasMet)
            {
                GameFlags.Instance.SetFlag(flagKey, true);
            }
        }
        else
        {
            PlayConversation(defaultConversationID);
        }
    }
    
    public void PlayConversation(string id)
    {
        DialogueData dialogueToPlay = GetDialogueByID(id);
        
        if (dialogueToPlay != null)
        {
            DialogueUI ui = FindObjectOfType<DialogueUI>();
            if (ui != null)
            {
                ui.SetCurrentNPC(transform);
            }

          
            if (ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartDialogue(dialogueToPlay);
            }
            else
            {
                Debug.LogError($"DialogueRunner not found! Cannot play conversation '{id}'");
            }
        }
        else
        {
            Debug.LogWarning($"NPC '{gameObject.name}' could not find conversation with ID: {id}");
        }
    }

   
    private DialogueData GetDialogueByID(string id)
    {
        foreach (var entry in allConversations)
        {
            if (entry.conversationID == id)
            {
                return entry.dialogue;
            }
        }
        return null;
    }
    
   
    public void SetActiveConversation(string newID)
    {
        defaultConversationID = newID;
        Debug.Log($"NPC '{gameObject.name}' default conversation changed to: {newID}");
    }

  
    public bool HasConversation(string id)
    {
        return GetDialogueByID(id) != null;
    }

   
    public void ResetMetFlag()
    {
        if (!string.IsNullOrEmpty(flagKey))
        {
            GameFlags.Instance.SetFlag(flagKey, false);
            Debug.Log($"Reset 'met' flag for {gameObject.name}");
        }
    }
    
}