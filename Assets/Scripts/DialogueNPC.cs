using System.Collections.Generic;
using UnityEngine;

public class DialogueNPC : MonoBehaviour
{
    [Header("Conversation Settings")]
    public string defaultConversationID = "";

    [Header("Conversation Registry")]
    public List<DialogueEntry> allConversations;

   
    public void Interact()
    {
    
        PlayConversation(defaultConversationID);
    }

  
    public void PlayConversation(string id)
    {
        Question questionToPlay = GetQuestionByID(id);
        
        if (questionToPlay != null)
        {
            ConversationManager.Instance.StartConversation(questionToPlay);
        }
        else
        {
            Debug.LogWarning($"NPC '{gameObject.name}' could not find conversation with ID: {id}");
        }
    }

    private Question GetQuestionByID(string id)
    {
        foreach (var entry in allConversations)
        {
            if (entry.conversationID == id)
            {
                return entry.question;
            }
        }
        return null;
    }
    
    // Helper to change the default conversation easily
    public void SetActiveConversation(string newID)
    {
        defaultConversationID = newID;
    }
}
