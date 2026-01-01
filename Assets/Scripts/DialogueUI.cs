using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
[Header("Position Settings")]
    public float heightOffset = 2.5f;
    public Transform playerTransform;

    [Header("NPC Dialogue Settings")]
    [SerializeField] private GameObject npcBubble;
    [SerializeField] private TextMeshProUGUI npcText;

    [Header("Player Dialogue Settings")]
    [SerializeField] private GameObject playerBubble;
    [SerializeField] private TextMeshProUGUI playerText;

    [Header("Shared References")]
    public Button[] dialogueButtons;

    private Transform currentNpcTransform;

    public void SetCurrentNPC(Transform npc)
    {
        currentNpcTransform = npc;
    }

    public void DisplayDialogue(Question question)
    {
        if (question == null) return;

        // 1. Disable bubbles initially
        if (npcBubble != null) npcBubble.SetActive(false);
        if (playerBubble != null) playerBubble.SetActive(false);

        // 2. Hide buttons (since we are auto-playing)
        foreach (var btn in dialogueButtons) btn.gameObject.SetActive(false);

        // 3. Show the correct bubble
        if (question.SpeakerName == "Player")
        {
            ShowPlayerDialogue(question.questionText);
        }
        else
        {
            ShowNPCDialogue(question.questionText);
        }
    }

    private void ShowNPCDialogue(string text)
    {
        if (npcBubble != null)
        {
            // Position Logic
            if (currentNpcTransform != null)
            {
                npcBubble.transform.position = currentNpcTransform.position + Vector3.up * heightOffset;
            }

            // Text Logic
            if (npcText != null) npcText.text = text;
            npcBubble.SetActive(true);

            // FORCE REFRESH: Tells Unity to resize the ContentSizeFitter NOW, not next frame
            LayoutRebuilder.ForceRebuildLayoutImmediate(npcBubble.GetComponent<RectTransform>());
        }
    }

    private void ShowPlayerDialogue(string text)
    {
        if (playerBubble != null)
        {
            // Position Logic
            if (playerTransform != null)
            {
                playerBubble.transform.position = playerTransform.position + Vector3.up * heightOffset;
            }

            // Text Logic
            if (playerText != null) playerText.text = text;
            playerBubble.SetActive(true);

            // FORCE REFRESH
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerBubble.GetComponent<RectTransform>());
        }
    }
}
