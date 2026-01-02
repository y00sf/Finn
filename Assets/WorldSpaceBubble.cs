using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldSpaceBubble : MonoBehaviour
{
    [Header("References")]
    public Transform targetNPC; 
    public Vector3 offset = new Vector3(0, 2f, 0); 
    public TextMeshProUGUI textComponent;
    public RectTransform bubbleRect;

    [Header("Settings")]
    public bool clampToScreen = true; 

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (targetNPC == null) return;

        
        Vector3 screenPos = mainCam.WorldToScreenPoint(targetNPC.position + offset);

       
        if (screenPos.z < 0) 
        {
            bubbleRect.position = new Vector3(-1000, -1000, 0); 
        }
        else
        {
            
            if (clampToScreen)
            {
                
                float padding = 50f;
                screenPos.x = Mathf.Clamp(screenPos.x, padding, Screen.width - padding);
                screenPos.y = Mathf.Clamp(screenPos.y, padding, Screen.height - padding);
            }
            
            bubbleRect.position = screenPos;
        }
    }

    
    public void ShowDialogue(Transform npc, string text)
    {
        targetNPC = npc;
        textComponent.text = text;
        gameObject.SetActive(true);
        
      
        LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
    }

    public void HideDialogue()
    {
        gameObject.SetActive(false);
    }
}