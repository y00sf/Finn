using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct LetterSoundMapping
{
    public string character;
    public AudioClip clip;
}

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AudioSource))]
public class DynamicChatBubble : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textComponent;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float maxWidth = 400f;
    public float minWidth = 100f;
    public RectOffset padding;
    public float bubbleGrowDuration = 0.15f;

    public bool stopSoundOnSpace = true;

    public List<LetterSoundMapping> letterSoundList;

    public bool IsTyping { get; private set; }
    private string fullText;
    private Coroutine typingCoroutine;

    private Dictionary<char, AudioClip> audioDictionary;

    private RectTransform rectTransform;
    private ContentSizeFitter bubbleSizeFitter;
    private VerticalLayoutGroup bubbleLayoutGroup;
    private LayoutElement textLayoutElement;
    private AudioSource audioSource;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();
        
        // Initialize padding if not set in inspector
        if (padding == null)
        {
            padding = new RectOffset(20, 20, 15, 15);
        }
        
        SetupAudio();
        BuildAudioDictionary();
        SetupComponents();
    }

    private void BuildAudioDictionary()
    {
        audioDictionary = new Dictionary<char, AudioClip>();

        foreach (var mapping in letterSoundList)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.clip != null)
            {
                char c = mapping.character.ToLower()[0];

                if (!audioDictionary.ContainsKey(c))
                {
                    audioDictionary.Add(c, mapping.clip);
                }
            }
        }
    }

    private void SetupAudio()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
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

    public void SetText(string message, bool animate = true)
    {
        fullText = message;

        if (animate)
        {
            textComponent.text = string.Empty;
            Vector2 preferredValues = textComponent.GetPreferredValues(fullText);
            float targetWidth = Mathf.Clamp(preferredValues.x, minWidth, maxWidth);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(GrowAndTypeRoutine(targetWidth));
        }
        else
        {
            textComponent.text = fullText;
            UpdateBubbleSize(fullText);
        }
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        IsTyping = false;
        textComponent.text = fullText;
    }

    private IEnumerator TypeRoutine()
    {
        IsTyping = true;
        yield return null;

        int totalChars = fullText.Length;
        for (int i = 0; i <= totalChars; i++)
        {
            textComponent.text = fullText.Substring(0, i);

            if (i > 0)
            {
                char visibleChar = fullText[i - 1];
                int rando = Random.Range(0, 2);
                if (rando == 0)
                {
                    PlaySoundForChar(visibleChar);
                }
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        IsTyping = false;
    }

    private void PlaySoundForChar(char c)
    {
        if (stopSoundOnSpace && char.IsWhiteSpace(c)) return;

        char lowerChar = char.ToLower(c);

        if (audioDictionary.TryGetValue(lowerChar, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator GrowAndTypeRoutine(float targetWidth)
    {
        float startWidth = textLayoutElement.preferredWidth;
        if (startWidth <= 0f) startWidth = minWidth;

        float elapsed = 0f;
        float duration = Mathf.Max(0.001f, bubbleGrowDuration);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float w = Mathf.Lerp(startWidth, targetWidth, t);
            textLayoutElement.preferredWidth = w;
            textLayoutElement.preferredHeight = -1;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            elapsed += Time.deltaTime;
            yield return null;
        }

        textLayoutElement.preferredWidth = targetWidth;
        textLayoutElement.preferredHeight = -1;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        yield return StartCoroutine(TypeRoutine());
    }

    private void UpdateBubbleSize(string messageToCheck)
    {
        Vector2 preferredValues = textComponent.GetPreferredValues(messageToCheck);
        float targetWidth = Mathf.Clamp(preferredValues.x, minWidth, maxWidth);
        textLayoutElement.preferredWidth = targetWidth;
        textLayoutElement.preferredHeight = -1;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}