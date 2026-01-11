using System;
using System.Collections;
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

    [Header("Settings")]
    public float autoAdvanceDelay = 2.0f; 

    [Header("Conversations")]
    public List<DialogueEntry> conversation;
    
    [Header("Data")]
    public Question currentQuestion;
    private Coroutine autoAdvanceCoroutine;

    public UnityEvent OnConversationStart;
    public UnityEvent OnConversationEnd;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartConversation(Question startingQuestion, Transform npcSpeaker)
    {
        if (startingQuestion == null) return;
        currentQuestion = startingQuestion;
        
        if (dialogueUI != null) dialogueUI.SetCurrentNPC(npcSpeaker);
        if (dialogueCanvas != null) dialogueCanvas.SetActive(true);

        OnConversationStart?.Invoke();
        UpdateUI();
    }

    private void EndConversation()
    {
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
        
        currentQuestion = null;
        OnConversationEnd?.Invoke();
    }

    private void UpdateUI()
    {
        if (dialogueUI != null && currentQuestion != null)
        {
            dialogueUI.DisplayDialogue(currentQuestion);

            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(WaitAndAutoAdvance());
        }
    }

   
    private IEnumerator WaitAndAutoAdvance()
    {
       
        yield return null; 

        
        while (dialogueUI != null && dialogueUI.IsAnyBubbleTyping)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(autoAdvanceDelay);

        
        if (currentQuestion.choices != null && currentQuestion.choices.Count > 0)
        {
            AdvanceConversation(0);
        }
        else
        {
            EndConversation();
        }
    }

    public void AdvanceConversation(int choiceIndex)
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
}
