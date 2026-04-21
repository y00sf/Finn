using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using ModularMotion.Core;

public class DialogueUI : MonoBehaviour
{
    public event Action<char> OnNpcCharacterRevealed;

    [Header("Input Settings")]
    [Tooltip("Define your key here. Click (+) -> Add Binding -> Press 'E'")] 
    public InputAction advanceAction; 

    [Header("Position Settings")]
    [FormerlySerializedAs("heightOffset")]
    public float npcHeightOffset = 2.5f;
    public float playerHeightOffset = 2.5f;
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
        if (npcBubble != null)
        {
            npcBubble.OnCharacterRevealed += HandleNpcCharacterRevealed;
        }

        if (mainCanvasContainer != null) mainCanvasContainer.SetActive(false);
        HideBubbles();
    }


    private void LateUpdate()
    {
     
        if (mainCanvasContainer != null && !mainCanvasContainer.activeSelf) return;

        if (npcBubble != null && npcBubble.gameObject.activeSelf && currentNpcTransform != null)
        {
            PositionBubble(npcBubble, currentNpcTransform, npcHeightOffset);
        }
        
        if (playerBubble != null && playerBubble.gameObject.activeSelf && playerTransform != null)
        {
            PositionBubble(playerBubble, playerTransform, playerHeightOffset);
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

    private void OnDestroy()
    {
        if (npcBubble != null)
        {
            npcBubble.OnCharacterRevealed -= HandleNpcCharacterRevealed;
        }
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

    private void HandleNpcCharacterRevealed(char revealedCharacter)
    {
        OnNpcCharacterRevealed?.Invoke(revealedCharacter);
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
            npcBubble.gameObject.SetActive(true);
            PositionBubble(npcBubble, currentNpcTransform, npcHeightOffset);
            npcBubble.SetText(text, true); 
        }
    }

    private void ShowPlayerDialogue(string text)
    {
        if (playerBubble != null)
        {
            playerBubble.gameObject.SetActive(true);
            PositionBubble(playerBubble, playerTransform, playerHeightOffset);
            playerBubble.SetText(text, true);
        }
    }

    private void PositionBubble(DynamicChatBubble bubble, Transform anchor, float heightOffset)
    {
        if (bubble == null || anchor == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 worldPos = anchor.position + Vector3.up * heightOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        bubble.SetAnchorScreenPosition(screenPos);
    }
}
