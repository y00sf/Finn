using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Define your key here. Click (+) -> Add Binding -> Press 'E'")] 
    public InputAction advanceAction; 

    [Header("Position Settings")]
    public float heightOffset = 2.5f;
    public Transform playerTransform;

    [Header("Bubble References")]
    [SerializeField] private DynamicChatBubble npcBubble;
    [SerializeField] private DynamicNPCTag npcTag;
    [SerializeField] private DynamicChatBubble playerBubble;

    [Header("Container Reference")]
    public GameObject mainCanvasContainer; 

   

    public event Action OnContinueClicked;

    private Transform currentNpcTransform;

    private void Start()
    {
      
        
        if (mainCanvasContainer != null) mainCanvasContainer.SetActive(false);
        HideBubbles();
    }


    private void LateUpdate()
    {
     
        if (mainCanvasContainer != null && !mainCanvasContainer.activeSelf) return;

 
        Camera mainCam = Camera.main;
        if (mainCam == null) return;
        
        if (npcBubble != null && npcBubble.gameObject.activeSelf && currentNpcTransform != null)
        {
            Vector3 worldPos = currentNpcTransform.position + Vector3.up * heightOffset;
            npcBubble.transform.position = mainCam.WorldToScreenPoint(worldPos);
        }
        
        if (playerBubble != null && playerBubble.gameObject.activeSelf && playerTransform != null)
        {
            Vector3 worldPos = playerTransform.position + Vector3.up * heightOffset;
            playerBubble.transform.position = mainCam.WorldToScreenPoint(worldPos);
        }
    }
   

    private void OnEnable()
    {
        advanceAction.Enable();
        advanceAction.performed += OnAdvanceInput;
    }

    private void OnDisable()
    {
        advanceAction.performed -= OnAdvanceInput;
        advanceAction.Disable();
    }

    private void OnAdvanceInput(InputAction.CallbackContext context)
    {
        if (mainCanvasContainer != null && mainCanvasContainer.activeSelf)
        {
        
            if (npcBubble != null && npcBubble.gameObject.activeSelf && npcBubble.IsTyping)
            {
                npcBubble.SkipTyping();
                return; 
            }
        
            if (playerBubble != null && playerBubble.gameObject.activeSelf && playerBubble.IsTyping)
            {
                playerBubble.SkipTyping();
                return; 
            }
            
            TriggerContinue();
        }
    }

    private void TriggerContinue()
    {
        OnContinueClicked?.Invoke();
    }

    public void SetCurrentNPC(Transform npc)
    {
        currentNpcTransform = npc;
    }

    public void ShowLine(string text, string speakerName, NPC npc)
    {
        if (mainCanvasContainer != null) mainCanvasContainer.SetActive(true);

        HideBubbles();
        
        if (speakerName == "Player")
        {
            ShowPlayerDialogue(text);
        }
        else
        {
            ShowNPCDialogue(text);
            npcTag.ChangeTagInfo(npc);
        }
    }

    public void Hide()
    {
        HideBubbles();
       
        if (mainCanvasContainer != null) mainCanvasContainer.SetActive(false);
    }

    private void HideBubbles()
    {
        if (npcBubble != null) npcBubble.gameObject.SetActive(false);
        if (playerBubble != null) playerBubble.gameObject.SetActive(false);
    }

    private void ShowNPCDialogue(string text)
    {
        if (npcBubble != null)
        {
            if (currentNpcTransform != null)
                npcBubble.transform.position = currentNpcTransform.position + Vector3.up * heightOffset;

            npcBubble.gameObject.SetActive(true);
           
            npcBubble.SetText(text, true); 
        }
    }

    private void ShowPlayerDialogue(string text)
    {
        if (playerBubble != null)
        {
            if (playerTransform != null)
                playerBubble.transform.position = playerTransform.position + Vector3.up * heightOffset;

            playerBubble.gameObject.SetActive(true);
            playerBubble.SetText(text, true);
        }
    }
}