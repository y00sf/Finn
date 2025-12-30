using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldSpaceBubble : MonoBehaviour
{
    [Header("References")]
    public Transform targetNPC; // The head of the NPC
    public Vector3 offset = new Vector3(0, 2f, 0); // Height adjustment
    public TextMeshProUGUI textComponent;
    public RectTransform bubbleRect;

    [Header("Settings")]
    public bool clampToScreen = true; // Keep box inside screen edges

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        // Hide initially
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (targetNPC == null) return;

        // 1. Convert 3D NPC position to 2D Screen position
        Vector3 screenPos = mainCam.WorldToScreenPoint(targetNPC.position + offset);

        // 2. Hide if behind the camera (prevents glitching)
        if (screenPos.z < 0) 
        {
            bubbleRect.position = new Vector3(-1000, -1000, 0); // Move offscreen
        }
        else
        {
            // 3. Apply Position
            if (clampToScreen)
            {
                // Optional: Keep the box within the screen borders
                float padding = 50f;
                screenPos.x = Mathf.Clamp(screenPos.x, padding, Screen.width - padding);
                screenPos.y = Mathf.Clamp(screenPos.y, padding, Screen.height - padding);
            }
            
            bubbleRect.position = screenPos;
        }
    }

    // Call this from your NPC script
    public void ShowDialogue(Transform npc, string text)
    {
        targetNPC = npc;
        textComponent.text = text;
        gameObject.SetActive(true);
        
        // Force the layout to update instantly (prevents 1-frame flickering)
        LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
    }

    public void HideDialogue()
    {
        gameObject.SetActive(false);
    }
}