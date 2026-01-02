using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
   [Header("Position Settings")]
    public float heightOffset = 2.5f;
    public Transform playerTransform;

    [Header("Bubble References")]
    [SerializeField] private DynamicChatBubble npcBubble;
    [SerializeField] private DynamicChatBubble playerBubble;

    [Header("Shared References")]
    public Button[] dialogueButtons;

    private Transform currentNpcTransform;

    
    public bool IsAnyBubbleTyping
    {
        get
        {
            if (npcBubble != null && npcBubble.gameObject.activeSelf && npcBubble.IsTyping) return true;
            if (playerBubble != null && playerBubble.gameObject.activeSelf && playerBubble.IsTyping) return true;
            return false;
        }
    }

    public void SetCurrentNPC(Transform npc)
    {
        currentNpcTransform = npc;
    }

    public void DisplayDialogue(Question question)
    {
        if (question == null) return;

        if (npcBubble != null) npcBubble.gameObject.SetActive(false);
        if (playerBubble != null) playerBubble.gameObject.SetActive(false);
        foreach (var btn in dialogueButtons) btn.gameObject.SetActive(false);

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
            if (currentNpcTransform != null)
                npcBubble.transform.position = currentNpcTransform.position + Vector3.up * heightOffset;

            npcBubble.gameObject.SetActive(true);
            npcBubble.SetText(text);
        }
    }

    private void ShowPlayerDialogue(string text)
    {
        if (playerBubble != null)
        {
            if (playerTransform != null)
                playerBubble.transform.position = playerTransform.position + Vector3.up * heightOffset;

            playerBubble.gameObject.SetActive(true);
            playerBubble.SetText(text);
        }
    }
}
