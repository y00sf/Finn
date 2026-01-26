using System;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [Header("Scripts to Toggle")]
    [SerializeField] private FishingCaster fishingCaster;
    [SerializeField] private PlayerMovement playerInput;
    [SerializeField] private PlayerInteraction playerInteraction;

    private DialogueData currentDialogue;
    private DialogueNode currentNode;
    private NPC currentNPC;
    private bool isRunning = false;
    
    [SerializeField] public event Action<DialogueData> OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action<DialogueNode> OnLineShow;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindObjectOfType<DialogueUI>();

        if (dialogueUI != null)
        {
            dialogueUI.OnContinueClicked += HandleContinue;
        }
    }
    
    public void StartDialogue(DialogueData dialogue, NPC npc, Transform npcTransform = null)
    {
        if (dialogue == null || !dialogue.Validate()) 
        {
            Debug.LogError("DialogueData is null or invalid!");
            return;
        }

        if (isRunning) return;

        if (dialogue.lines == null || dialogue.lines.Count == 0) return;
    
        currentDialogue = dialogue;
        currentNode = dialogue.lines[0];
        currentNPC = npc;
        isRunning = true;
        
        if (dialogueUI != null && npcTransform != null)
        {
            dialogueUI.SetCurrentNPC(npcTransform);
        }

        OnDialogueStart?.Invoke(dialogue);
        SetPlayerScriptsEnabled(false);
        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }
        
        if (currentNode.setFlag && !string.IsNullOrEmpty(currentNode.flagToSet))
        {
            GameFlags.Instance.SetFlag(currentNode.flagToSet, currentNode.flagValue);
        }

        OnLineShow?.Invoke(currentNode);
        
        if (dialogueUI != null)
        {
            dialogueUI.ShowLine(currentNode.text, currentNode.speakerName, currentNPC);
        }
    }

    private void HandleContinue()
    {
        if (!isRunning || currentNode == null) return;

        string nextID = currentNode.nextNodeID;
        
        if (currentNode.useCondition && !string.IsNullOrEmpty(currentNode.requiredFlag))
        {
            bool flagValue = GameFlags.Instance.GetFlag(currentNode.requiredFlag);
            
            nextID = flagValue ? currentNode.trueNodeID : currentNode.falseNodeID;
        
            Debug.Log($"[Branching] Flag '{currentNode.requiredFlag}' is {flagValue}. Going to: {nextID}");
        }

        if (!string.IsNullOrEmpty(nextID))
        {
            DialogueNode nextNode = currentDialogue.GetNode(nextID);
            if (nextNode != null)
            {
                currentNode = nextNode;
                ShowCurrentNode();
            }
            else
            {
                Debug.LogWarning($"Node '{nextID}' not found. Ending dialogue.");
                EndDialogue();
            }
        }
        else
        {
            Debug.Log("No next node - ending dialogue");
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (!isRunning) return;

        Debug.Log("Ending dialogue...");
    
        isRunning = false;
        currentDialogue = null;
        currentNode = null;

        if (dialogueUI != null) dialogueUI.Hide();
        
        StartCoroutine(EnablePlayerAfterDelay());

        OnDialogueEnd?.Invoke();
    }
    
    private System.Collections.IEnumerator EnablePlayerAfterDelay()
    {
   
        yield return new WaitForSeconds(0.2f);
    
        SetPlayerScriptsEnabled(true);
    }

    private void OnDestroy()
    {
        if (dialogueUI != null) dialogueUI.OnContinueClicked -= HandleContinue;
    }
    
    private void SetPlayerScriptsEnabled(bool enabled)
    {
        if (fishingCaster != null) fishingCaster.enabled = enabled;
        if (playerInput != null) playerInput.enabled = enabled;
        
      
        if (playerInteraction != null) playerInteraction.enabled = enabled;
    }
    
    public bool IsDialogueRunning()
    {
        return isRunning;
    }
}