using System;
using TMPro;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    [Header("Dependencies")]
    public DialogueUI dialogueUI;
    public GameObject dialogueCanvas;

    [Header("Data")]
    public Question currentQuestion;

    private void Start()
    {
        if(dialogueCanvas != null) dialogueCanvas.SetActive(false);
    }

    
    public void StartConversation(Question startingQuestion)
    {
        currentQuestion = startingQuestion;
        
        if(dialogueCanvas != null) dialogueCanvas.SetActive(true);
        
        UpdateUI();
    }

    private void OnEnable()
    {
        ChoiceHandler.OnChoiceMade += AdvanceConversation;
    }

    private void OnDisable()
    {
        ChoiceHandler.OnChoiceMade -= AdvanceConversation;
    }

    private void AdvanceConversation(int choiceIndex)
    {
        if (currentQuestion == null) return;
        
        if (choiceIndex >= currentQuestion.choices.Count) return;

        Question nextNode = currentQuestion.choices[choiceIndex].nextDialogue;

        if (nextNode != null)
        {
            currentQuestion = nextNode;
            UpdateUI();
        }
        else
        {
            EndConversation();
        }
    }

    private void UpdateUI()
    {
        if (dialogueUI != null && currentQuestion != null)
        {
            dialogueUI.DisplayDialogue(currentQuestion);
        }
    }

    private void EndConversation()
    {
        Debug.Log("Conversation Ended.");
        if(dialogueCanvas != null) dialogueCanvas.SetActive(false);
        currentQuestion = null;
    }
}
