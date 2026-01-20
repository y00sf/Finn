using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WorldSpaceBubble : MonoBehaviour
{
    [Header("References")]
    public Transform targetNPC;
    public Vector3 offset = new Vector3(0, 2f, 0);
    public DynamicChatBubble chatBubble; 

    [Header("Legacy References (if not using DynamicChatBubble)")]
    public TextMeshProUGUI textComponent;
    public RectTransform bubbleRect;

    [Header("Settings")]
    public bool clampToScreen = true;
    public float padding = 50f;

    private Camera mainCam;

    public bool IsTyping
    {
        get
        {
            if (chatBubble != null)
                return chatBubble.IsTyping;
            return false;
        }
    }

    void Start()
    {
        mainCam = Camera.main;
        
        if (chatBubble == null)
        {
            chatBubble = GetComponentInChildren<DynamicChatBubble>();
        }

    
        if (chatBubble != null)
        {
            textComponent = chatBubble.textComponent;
            bubbleRect = chatBubble.GetComponent<RectTransform>();
        }

        if (bubbleRect == null)
        {
            bubbleRect = GetComponent<RectTransform>();
        }

        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (targetNPC == null) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(targetNPC.position + offset);
        
        if (screenPos.z < 0)
        {
            bubbleRect.position = new Vector3(-10000, -10000, 0);
        }
        else
        {
            if (clampToScreen)
            {
                screenPos.x = Mathf.Clamp(screenPos.x, padding, Screen.width - padding);
                screenPos.y = Mathf.Clamp(screenPos.y, padding, Screen.height - padding);
            }

            bubbleRect.position = screenPos;
        }
    }
    
    public void ShowDialogue(Transform npc, string text)
    {
        targetNPC = npc;
        gameObject.SetActive(true);
        
        if (chatBubble != null)
        {
            chatBubble.SetText(text);
        }
        else if (textComponent != null)
        {
            // Fallback to instant text
            textComponent.text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
        }
    }
    
    public void HideDialogue()
    {
        gameObject.SetActive(false);
    }
}