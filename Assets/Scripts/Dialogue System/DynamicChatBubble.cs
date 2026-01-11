using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DynamicChatBubble : MonoBehaviour
{
  [Header("Settings")]
    public float maxBubbleWidth = 400f;
    public float typingSpeed = 0.04f;

    [Header("References")]
    public TextMeshProUGUI textComponent;
    public Image bubbleImage;
    
    public bool IsTyping { get; private set; }

    private LayoutElement textLayoutElement;
    private RectTransform rectTransform;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (textComponent == null) textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (bubbleImage == null) bubbleImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (textComponent != null)
        {
            textLayoutElement = textComponent.GetComponent<LayoutElement>();
            if (textLayoutElement == null) 
            {
                textLayoutElement = textComponent.gameObject.AddComponent<LayoutElement>();
            }
        }
    }
    

    public void SetText(string message)
    {
        if (textComponent == null || textLayoutElement == null) return;
       

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        textComponent.text = message;
        
        IsTyping = true;
        
        if (textLayoutElement != null)
        {
            textLayoutElement.enabled = false;
            textLayoutElement.preferredWidth = -1;
        }

        float textWidth = textComponent.GetPreferredValues(message).x;

        if (textWidth > maxBubbleWidth)
        {
            if (textLayoutElement != null)
            {
                textLayoutElement.enabled = true;
                textLayoutElement.preferredWidth = maxBubbleWidth;
            }
        }

        if (rectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        typingCoroutine = StartCoroutine(TypewriterRoutine());
    }

    private IEnumerator TypewriterRoutine()
    {
        textComponent.maxVisibleCharacters = 0;
        yield return null;

        int totalVisibleCharacters = textComponent.textInfo.characterCount;
        int counter = 0;

        while (counter < totalVisibleCharacters + 1)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);
            textComponent.maxVisibleCharacters = visibleCount;
            counter++;
            yield return new WaitForSeconds(typingSpeed);
        }

        
        IsTyping = false;
    }
}