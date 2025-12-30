using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public struct DialogueEntry
{
    public string conversationID;
    public Question question;
}
public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; }
    [Header("Dependencies")]
   [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private GameObject dialogueCanvas;

    [Header("Conversations")]
    public List<DialogueEntry> conversation;
    [Header("Data")]
    public Question currentQuestion;
    private bool isConversationActive = false;

    public UnityEvent OnConversationStart;
    public UnityEvent OnConversationEnd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(false);
        }
    }

    
    public void StartConversation(Question startingQuestion)
    {
        if (startingQuestion != null)
        {
            Debug.Log("Starting conversation with a null question");
            return;
        }
        currentQuestion = startingQuestion;
        isConversationActive = true;
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
        }
        OnConversationStart?.Invoke();
        UpdateUI();
    }
    
    private void EndConversation()
    {
        Debug.Log("Conversation Ended.");
        isConversationActive = false;
        currentQuestion = null;
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(false);
        }
        OnConversationEnd?.Invoke();
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

   


    private void SelectNextConversation(int choiceIndex)
    {
        for (int i = 0; i < conversation.Count; i++)
        {
            if (i == choiceIndex)
            {
                currentQuestion = conversation[i].question;
            }
        }
    }
}
