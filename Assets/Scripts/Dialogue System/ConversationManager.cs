using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ConversationManager : MonoBehaviour
{

    public static ConversationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isRunning = false;
    
    public event Action<DialogueData> OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action<DialogueNode> OnLineShow;

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
        if (dialogueUI == null)
        {
            dialogueUI = FindObjectOfType<DialogueUI>();
        }

        if (dialogueUI != null)
        {
            dialogueUI.OnContinueClicked += HandleContinue;
        }
    }
    
    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogError("[DialogueRunner] Cannot start null dialogue");
            return;
        }

        if (!dialogue.Validate())
        {
            Debug.LogError($"[DialogueRunner] Dialogue '{dialogue.dialogueName}' failed validation");
            return;
        }

        if (isRunning)
        {
            Debug.LogWarning("[DialogueRunner] Dialogue already running, stopping current dialogue");
            EndDialogue();
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isRunning = true;

        OnDialogueStart?.Invoke(dialogue);
        Debug.Log($"[DialogueRunner] Starting dialogue: {dialogue.dialogueName}");

        ShowCurrentLine();
    }

   
    private void ShowCurrentLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueNode line = currentDialogue.lines[currentLineIndex];
        
        if (line == null)
        {
            Debug.LogError($"[DialogueRunner] Null line at index {currentLineIndex}");
            EndDialogue();
            return;
        }

    
        if (line.setFlag && !string.IsNullOrEmpty(line.flagToSet))
        {
            GameFlags.Instance.SetFlag(line.flagToSet, line.flagValue);
            Debug.Log($"[DialogueRunner] Set flag '{line.flagToSet}' to {line.flagValue}");
        }

        OnLineShow?.Invoke(line);
        Debug.Log($"[DialogueRunner] Showing line {currentLineIndex}: '{line.text}'");

        if (dialogueUI != null)
        {
            dialogueUI.ShowLine(line.text, line.speakerName);
        }
    }

  
    private void HandleContinue()
    {
        if (!isRunning) return;

        currentLineIndex++;
        ShowCurrentLine();
    }


    public void GoToLine(string lineID)
    {
        if (currentDialogue == null)
        {
            Debug.LogError("[DialogueRunner] No dialogue running");
            return;
        }

        for (int i = 0; i < currentDialogue.lines.Count; i++)
        {
            if (currentDialogue.lines[i].id == lineID)
            {
                currentLineIndex = i;
                ShowCurrentLine();
                return;
            }
        }

        Debug.LogError($"[DialogueRunner] Line ID '{lineID}' not found");
    }

 
    public void EndDialogue()
    {
        if (!isRunning) return;

        Debug.Log("[DialogueRunner] Ending dialogue");

        isRunning = false;
        currentDialogue = null;
        currentLineIndex = 0;

        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        OnDialogueEnd?.Invoke();
    }

    public bool IsDialogueRunning()
    {
        return isRunning;
    }

    private void OnDestroy()
    {
        if (dialogueUI != null)
        {
            dialogueUI.OnContinueClicked -= HandleContinue;
        }
    }
}
