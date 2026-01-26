using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicNPCTag : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float maxWidth = 400f;
    public float minWidth = 100f;
    public RectOffset padding;
   
    private string fullText;
    private RectTransform rectTransform;
    private ContentSizeFitter bubbleSizeFitter;
    private VerticalLayoutGroup bubbleLayoutGroup;
    private LayoutElement textLayoutElement;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Initialize padding if not set in inspector
        if (padding == null)
        {
            padding = new RectOffset(20, 20, 15, 15);
        }
        
        SetupComponents();
    }
    
    private void SetupComponents()
    {
        bubbleSizeFitter = GetComponent<ContentSizeFitter>();
        if (bubbleSizeFitter == null) bubbleSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
    
        bubbleSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        bubbleSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    
        bubbleLayoutGroup = GetComponent<VerticalLayoutGroup>();
        if (bubbleLayoutGroup == null) bubbleLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
    
        bubbleLayoutGroup.childControlWidth = true;
        bubbleLayoutGroup.childControlHeight = true;
        bubbleLayoutGroup.childForceExpandWidth = false;
        bubbleLayoutGroup.childForceExpandHeight = false;
        bubbleLayoutGroup.padding = padding;
    
        textLayoutElement = textComponent.GetComponent<LayoutElement>();
        if (textLayoutElement == null) textLayoutElement = textComponent.gameObject.AddComponent<LayoutElement>();
    
        textComponent.enableWordWrapping = true;
        textComponent.overflowMode = TextOverflowModes.Overflow;
    }
    
    private void UpdateBubbleSize(string messageToCheck)
    {
        textComponent.enableWordWrapping = false;
        
        Vector2 preferredValues = textComponent.GetPreferredValues(messageToCheck);
    
        if (preferredValues.x > maxWidth)
        {
            textLayoutElement.preferredWidth = maxWidth;
            textComponent.enableWordWrapping = true;
        }
        else
        {
            textLayoutElement.preferredWidth = Mathf.Max(preferredValues.x, minWidth);
            textComponent.enableWordWrapping = false;
        }
    
        textLayoutElement.preferredHeight = -1;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void ChangeTagInfo(NPC npc)
    {
        if (npc == null) return;
        GetComponent<Image>().color = npc.npcBubbleColor;
        textComponent.color = npc.npcTextColor;
        textComponent.text = npc.npcName;
        UpdateBubbleSize(npc.npcName);
    }
}