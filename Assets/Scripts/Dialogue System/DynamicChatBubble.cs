using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ModularMotion;

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
    public event System.Action<char> OnCharacterRevealed;

    [Header("References")]
    public TextMeshProUGUI textComponent;
   [SerializeField] private UIMotion uiMotion;
    [SerializeField] private RectTransform tailAnchor;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float maxWidth = 400f;
    public float minWidth = 100f;
    public RectOffset padding;
    public float bubbleGrowDuration = 0.15f;

    public bool stopSoundOnSpace = true;

    [Header("Typewriter Commands")]
    public float slowSpeedMultiplier = 2f;
    public float fastSpeedMultiplier = 0.5f;

    [Header("Reveal Motion")]
    public float revealDuration = 0.08f;
    public float revealScaleFrom = 0.6f;
    public float revealYOffset = -6f;
    public AnimationCurve revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Shake Effect")]
    public float shakeSpeed = 18f;
    public float shakeDefaultMagnitude = 0.25f;

    [Header("Punctuation Pausing")]
    public float commaPause = 0.2f;
    public float periodPause = 0.5f;
    public float ellipsisPause = 0.6f;

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
    private readonly List<float> revealStartTimes = new List<float>();
    private readonly List<float> shakeMagnitudes = new List<float>();
    private int lastCharacterCount;
    private TMP_MeshInfo[] baseMeshInfo;
    private bool hasAnyShake;

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

    public void SetAnchorScreenPosition(Vector3 screenPosition)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        rectTransform.position = screenPosition;

        if (tailAnchor == null) return;

        Vector3 tailPosition = tailAnchor.position;
        rectTransform.position += screenPosition - tailPosition;
    }

    public void SetText(string message ,bool animate = true)
    {
        uiMotion.PlayFromStartTillEnd();
        fullText = message;
        string displayText = BuildFinalText(fullText);

        if (animate)
        {
            textComponent.text = string.Empty;
            ResetTypingState();
            Vector2 preferredValues = textComponent.GetPreferredValues(displayText);
            float targetWidth = Mathf.Clamp(preferredValues.x, minWidth, maxWidth);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(GrowAndTypeRoutine(targetWidth));
        }
        else
        {
            textComponent.text = BuildFinalText(fullText, shakeMagnitudes, out hasAnyShake);
            ResetRevealState();
            UpdateBubbleSize(displayText);
        }
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        IsTyping = false;
        StopAllCoroutines();
        textComponent.text = BuildFinalText(fullText, shakeMagnitudes, out hasAnyShake);
        ResetRevealState();
    }

    private IEnumerator TypeRoutine()
    {
        IsTyping = true;
        yield return null;

        StringBuilder displayBuilder = new StringBuilder();
        float currentSpeed = typingSpeed;
        bool capsMode = false;
        bool capNext = false;
        float currentShakeMagnitude = 0f;

        RegisterNewlyVisibleCharacters();

        for (int i = 0; i < fullText.Length; i++)
        {
            char currentChar = fullText[i];

            if (currentChar == '<')
            {
                int endIndex = fullText.IndexOf('>', i);
                if (endIndex != -1)
                {
                    string tag = fullText.Substring(i, endIndex - i + 1);

                    if (TryHandleCommandTag(tag, ref currentSpeed, ref capsMode, ref capNext, ref currentShakeMagnitude, out float waitSeconds))
                    {
                        if (waitSeconds > 0f)
                        {
                            yield return StartCoroutine(WaitWithReveal(waitSeconds));
                        }

                        i = endIndex;
                        continue;
                    }

                    displayBuilder.Append(tag);
                    textComponent.text = displayBuilder.ToString();
                    RegisterNewlyVisibleCharacters();
                    i = endIndex;
                    continue;
                }
            }

            char outputChar = ApplyCaps(currentChar, ref capsMode, ref capNext);
            displayBuilder.Append(outputChar);
            shakeMagnitudes.Add(currentShakeMagnitude);
            if (currentShakeMagnitude > 0f)
            {
                hasAnyShake = true;
            }
            textComponent.text = displayBuilder.ToString();
            RegisterNewlyVisibleCharacters();
            UpdateRevealAnimation();

            int rando = Random.Range(0, 2);
            if (rando == 0)
            {
                PlaySoundForChar(outputChar);
            }

            if (char.IsLetter(outputChar))
            {
                OnCharacterRevealed?.Invoke(outputChar);
            }

            float pauseAfterChar = GetPunctuationPause(i, currentChar);
            yield return StartCoroutine(WaitWithReveal(currentSpeed + pauseAfterChar));
        }

        IsTyping = false;
        yield return StartCoroutine(FinishRevealAnimation());
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

    private IEnumerator WaitWithReveal(float seconds)
    {
        float remaining = Mathf.Max(0f, seconds);

        while (remaining > 0f)
        {
            UpdateRevealAnimation();
            remaining -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FinishRevealAnimation()
    {
        if (revealDuration <= 0f) yield break;

        float endTime = Time.time + revealDuration;
        while (Time.time < endTime)
        {
            UpdateRevealAnimation();
            yield return null;
        }
    }

    private void RegisterNewlyVisibleCharacters()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount <= 0) return;

        if (revealStartTimes.Count < characterCount)
        {
            int missing = characterCount - revealStartTimes.Count;
            for (int i = 0; i < missing; i++)
            {
                revealStartTimes.Add(-1f);
            }
        }

        if (characterCount > lastCharacterCount)
        {
            float now = Time.time;
            for (int i = lastCharacterCount; i < characterCount; i++)
            {
                revealStartTimes[i] = now;
            }

            lastCharacterCount = characterCount;
        }

        baseMeshInfo = textInfo.CopyMeshInfoVertexData();
    }

    private float GetPunctuationPause(int index, char currentChar)
    {
        if (currentChar == ',')
        {
            return commaPause;
        }

        if (currentChar != '.')
        {
            return 0f;
        }

        bool isEllipsisEnd = index >= 2
            && fullText[index - 1] == '.'
            && fullText[index - 2] == '.'
            && (index + 1 >= fullText.Length || fullText[index + 1] != '.');

        if (isEllipsisEnd)
        {
            return ellipsisPause;
        }

        if (index + 1 < fullText.Length && fullText[index + 1] == '.')
        {
            return 0f;
        }

        return periodPause;
    }

    private void UpdateRevealAnimation()
    {
        bool hasReveal = revealDuration > 0f && revealStartTimes.Count > 0;
        bool hasShake = hasAnyShake;
        if (!hasReveal && !hasShake) return;

        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;
        if (characterCount == 0) return;

        if (baseMeshInfo == null || baseMeshInfo.Length == 0) return;

        float now = Time.time;
        float invDuration = hasReveal ? 1f / Mathf.Max(0.0001f, revealDuration) : 0f;
        float shakeTime = hasShake ? now * Mathf.Max(0.01f, shakeSpeed) : 0f;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;
            float t = 1f;
            if (hasReveal && i < revealStartTimes.Count)
            {
                float startTime = revealStartTimes[i];
                if (startTime >= 0f)
                {
                    t = Mathf.Clamp01((now - startTime) * invDuration);
                }
            }

            float eased = hasReveal && revealCurve != null ? revealCurve.Evaluate(t) : t;
            float scale = hasReveal ? Mathf.Lerp(revealScaleFrom, 1f, eased) : 1f;
            float yOffset = hasReveal ? Mathf.Lerp(revealYOffset, 0f, eased) : 0f;
            float shakeMagnitude = i < shakeMagnitudes.Count ? shakeMagnitudes[i] : 0f;

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            if (meshIndex >= baseMeshInfo.Length) continue;

            Vector3[] sourceVertices = baseMeshInfo[meshIndex].vertices;
            Vector3[] destVertices = textInfo.meshInfo[meshIndex].vertices;

            Vector3 mid = (sourceVertices[vertexIndex] + sourceVertices[vertexIndex + 2]) * 0.5f;
            Vector3 shakeOffset = Vector3.zero;
            if (shakeMagnitude > 0f)
            {
                float noiseX = Mathf.PerlinNoise(i * 0.13f, shakeTime) - 0.5f;
                float noiseY = Mathf.PerlinNoise(i * 0.13f + 23.7f, shakeTime) - 0.5f;
                shakeOffset = new Vector3(noiseX, noiseY, 0f) * (shakeMagnitude * 2f);
            }

            Vector3 offset = new Vector3(0f, yOffset, 0f) + shakeOffset;

            destVertices[vertexIndex] = mid + (sourceVertices[vertexIndex] - mid) * scale + offset;
            destVertices[vertexIndex + 1] = mid + (sourceVertices[vertexIndex + 1] - mid) * scale + offset;
            destVertices[vertexIndex + 2] = mid + (sourceVertices[vertexIndex + 2] - mid) * scale + offset;
            destVertices[vertexIndex + 3] = mid + (sourceVertices[vertexIndex + 3] - mid) * scale + offset;
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    private void ResetRevealState()
    {
        revealStartTimes.Clear();
        lastCharacterCount = 0;
        textComponent.ForceMeshUpdate();
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        baseMeshInfo = textComponent.textInfo.CopyMeshInfoVertexData();
    }

    private void ResetTypingState()
    {
        shakeMagnitudes.Clear();
        hasAnyShake = false;
        ResetRevealState();
    }

    private void Update()
    {
        if (!IsTyping && !hasAnyShake) return;
        UpdateRevealAnimation();
    }

    private void UpdateBubbleSize(string messageToCheck)
    {
        Vector2 preferredValues = textComponent.GetPreferredValues(messageToCheck);
        float targetWidth = Mathf.Clamp(preferredValues.x, minWidth, maxWidth);
        textLayoutElement.preferredWidth = targetWidth;
        textLayoutElement.preferredHeight = -1;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private string BuildFinalText(string rawText)
    {
        return BuildFinalText(rawText, null, out _);
    }

    private string BuildFinalText(string rawText, List<float> shakeOutput, out bool hasShake)
    {
        hasShake = false;
        if (string.IsNullOrEmpty(rawText))
        {
            shakeOutput?.Clear();
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        float currentSpeed = typingSpeed;
        bool capsMode = false;
        bool capNext = false;
        float currentShakeMagnitude = 0f;

        shakeOutput?.Clear();

        for (int i = 0; i < rawText.Length; i++)
        {
            char currentChar = rawText[i];

            if (currentChar == '<')
            {
                int endIndex = rawText.IndexOf('>', i);
                if (endIndex != -1)
                {
                    string tag = rawText.Substring(i, endIndex - i + 1);
                    if (TryHandleCommandTag(tag, ref currentSpeed, ref capsMode, ref capNext, ref currentShakeMagnitude, out _))
                    {
                        i = endIndex;
                        continue;
                    }

                    builder.Append(tag);
                    i = endIndex;
                    continue;
                }
            }

            builder.Append(ApplyCaps(currentChar, ref capsMode, ref capNext));
            if (shakeOutput != null)
            {
                shakeOutput.Add(currentShakeMagnitude);
                if (currentShakeMagnitude > 0f)
                {
                    hasShake = true;
                }
            }
        }

        return builder.ToString();
    }

    private bool TryHandleCommandTag(
        string tag,
        ref float currentSpeed,
        ref bool capsMode,
        ref bool capNext,
        ref float currentShakeMagnitude,
        out float waitSeconds)
    {
        waitSeconds = 0f;
        if (string.IsNullOrEmpty(tag)) return false;

        string lowerTag = tag.ToLowerInvariant();

        if (lowerTag == "<slow>")
        {
            currentSpeed = Mathf.Max(0.001f, typingSpeed * slowSpeedMultiplier);
            return true;
        }

        if (lowerTag == "<fast>")
        {
            currentSpeed = Mathf.Max(0.001f, typingSpeed * fastSpeedMultiplier);
            return true;
        }

        if (lowerTag == "<reset>")
        {
            currentSpeed = typingSpeed;
            capsMode = false;
            capNext = false;
            return true;
        }

        if (lowerTag == "<caps>")
        {
            capsMode = true;
            return true;
        }

        if (lowerTag == "</caps>")
        {
            capsMode = false;
            return true;
        }

        if (lowerTag == "<cap>")
        {
            capNext = true;
            return true;
        }

        if (lowerTag == "</shake>")
        {
            currentShakeMagnitude = 0f;
            return true;
        }

        if (lowerTag == "<shake>")
        {
            currentShakeMagnitude = Mathf.Max(0f, shakeDefaultMagnitude);
            return true;
        }

        if (TryParseCommandValue(lowerTag, "shake", out float shakeValue))
        {
            currentShakeMagnitude = Mathf.Max(0f, shakeValue);
            return true;
        }

        if (TryParseCommandValue(lowerTag, "speed", out float speedValue))
        {
            currentSpeed = Mathf.Max(0.001f, speedValue);
            return true;
        }

        if (TryParseCommandValue(lowerTag, "wait", out float waitValue))
        {
            waitSeconds = Mathf.Max(0f, waitValue);
            return true;
        }

        if (lowerTag == "<speed=default>" || lowerTag == "<speed:default>")
        {
            currentSpeed = typingSpeed;
            return true;
        }

        return false;
    }

    private bool TryParseCommandValue(string lowerTag, string key, out float value)
    {
        value = 0f;
        if (!lowerTag.StartsWith("<" + key, System.StringComparison.Ordinal)) return false;

        int equalsIndex = lowerTag.IndexOf('=');
        int colonIndex = lowerTag.IndexOf(':');
        int separatorIndex = equalsIndex >= 0 ? equalsIndex : colonIndex;
        if (separatorIndex < 0) return false;

        int endIndex = lowerTag.IndexOf('>', separatorIndex);
        if (endIndex < 0) return false;

        string numberText = lowerTag.Substring(separatorIndex + 1, endIndex - separatorIndex - 1).Trim();
        return float.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private char ApplyCaps(char input, ref bool capsMode, ref bool capNext)
    {
        char output = input;

        if (capNext)
        {
            output = char.ToUpperInvariant(output);
            capNext = false;
            return output;
        }

        if (capsMode)
        {
            output = char.ToUpperInvariant(output);
        }

        return output;
    }
}
