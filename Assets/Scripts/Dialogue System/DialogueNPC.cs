using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class DialogueEntry
{
    public string conversationID;
    public DialogueData dialogue;
}

[System.Serializable]
public class ConditionalSpeaking
{
    public string conversationID;
    public DialogueData dialogue;
}

public class DialogueNPC : MonoBehaviour
{
    [Header("Conversation Settings")]
    public string defaultConversationID = "greeting";

    [Header("Conversation Registry")]
    public List<DialogueEntry> allConversations = new List<DialogueEntry>();
    [Header("conditional speaking")]
    public List<ConditionalSpeaking> conditionalSpeakings = new List<ConditionalSpeaking>();

    [SerializeField] private float conditionalSpeakingChance = 1f;

    [SerializeField] private NPC npc;
    [Header("Bubble Anchor")]
    [SerializeField] private Transform bubbleAnchor;

    [Header("Talk Animation")]
    [SerializeField] private Transform Head;
    [SerializeField] private float talkAnimationDuration = 0.18f;
    [SerializeField] private Vector3 squashScale = new Vector3(0.9f, 1f, 1f);
    [SerializeField] private Vector3 stretchScale = new Vector3(1f, 1.1f, 1f);
        

    [Header("Optional Settings")]
    public bool useReturnConversation = false;
    public string returnConversationID = "return_greeting";
    
    private string flagKey;
    private DialogueUI cachedUI;
    private GameObject NPCGameObject;
    private Coroutine talkingCoroutine;
    private Vector3 originalHeadScale;
    private bool hasOriginalHeadScale;

    private void Start()
    {
        flagKey = $"met_{gameObject.name}";
        cachedUI = FindObjectOfType<DialogueUI>();

        if (NPCGameObject == null)
        {
            NPCGameObject = this.gameObject;
        }
    }
    
    public void Interact()
    {
        string conversationToPlay = defaultConversationID;

        if (useReturnConversation)
        {
            bool hasMet = GameFlags.Instance.GetFlag(flagKey);
            conversationToPlay = hasMet ? returnConversationID : defaultConversationID;
            
            if (!hasMet)
            {
                GameFlags.Instance.SetFlag(flagKey, true);
            }
        }

        PlayConversation(conversationToPlay);
    }
    
    public void PlayConversation(string id)
    {
        DialogueData dialogueToPlay = GetDialogueByID(id);
        
        if (dialogueToPlay != null)
        {
            if (ConversationManager.Instance != null)
            {
                Transform anchor = bubbleAnchor != null ? bubbleAnchor : transform;
                Look look = GetComponent<Look>();
                if (look != null)
                {
                    look.RotateNpcToPlayer();
                }
                ConversationManager.Instance.StartDialogue(dialogueToPlay, npc,NPCGameObject, anchor, transform);
            }
        }
        else
        {
            Debug.LogWarning($"NPC '{gameObject.name}' could not find conversation ID: {id}");
        }
    
    }
    
    public void SetActiveConversation(string newID)
    {
        defaultConversationID = newID;
        if (GetDialogueByID(newID) == null)
        {
            Debug.LogWarning($"[DialogueNPC] Warning: Switching to conversation ID '{newID}' but it was not found in the list.");
        }
        else
        {
            Debug.Log($"NPC '{gameObject.name}' default conversation changed to: {newID}");
        }
    }

    private DialogueData GetDialogueByID(string id)
    {
        foreach (var entry in allConversations)
        {
            if (entry.conversationID == id) return entry.dialogue;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (GetComponent<PathNPC>().GetCanWalk() == true)
            {
                if (Random.value <= conditionalSpeakingChance)
                {
                    Debug.Log($"[DialogueNPC] ConditionalSpeaking {other.name}");
                    ConditionalSpeaking();
                }
            }
        }
    }

    private void ConditionalSpeaking()
    {
        if (conditionalSpeakings.Count == 0) return;

        int random = Random.Range(0, conditionalSpeakings.Count);
        ConditionalSpeaking chosen = conditionalSpeakings[random];

        if (chosen.dialogue == null)
        {
            Debug.LogWarning($"[DialogueNPC] ConditionalSpeaking at index {random} has no dialogue assigned.");
            return;
        }

        chosen.dialogue.npc = npc;
    
        Transform anchor = bubbleAnchor != null ? bubbleAnchor : transform;
        Look look = GetComponent<Look>();
        if (look != null)
        {
            look.RotateNpcToPlayer();
        }
        ConversationManager.Instance.StartDialogue(chosen.dialogue, npc, gameObject, anchor, transform);
    }


    public void TalkAnimation()
    {
        if (Head == null)
        {
            return;
        }

        if (!hasOriginalHeadScale)
        {
            originalHeadScale = Head.localScale;
            hasOriginalHeadScale = true;
        }

        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            Head.localScale = originalHeadScale;
        }

        talkingCoroutine = StartCoroutine(Talking());
    }

    private IEnumerator Talking()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, talkAnimationDuration);
        Vector3 targetScale = Vector3.Scale(originalHeadScale, squashScale + stretchScale - Vector3.one);

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float blend = Mathf.Sin(normalizedTime * Mathf.PI);
            Head.localScale = Vector3.LerpUnclamped(originalHeadScale, targetScale, blend);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Head.localScale = originalHeadScale;
        talkingCoroutine = null;
    }

    private void OnDisable()
    {
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            talkingCoroutine = null;
        }

        if (Head != null && hasOriginalHeadScale)
        {
            Head.localScale = originalHeadScale;
        }
    }
}
