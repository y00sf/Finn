using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    
    public class DialogueUI : MonoBehaviour
    {
        public float npcHeightOffset = 2.5f;
        public float playerHeightOffset = 2.5f;
        public Transform playerTransform;
    
        [SerializeField] private WorldSpaceBubble npcBubble;
        [SerializeField] private WorldSpaceBubble playerBubble;
    
        [SerializeField] private bool waitForTypingToFinish = true;
        [SerializeField] private float autoHideDelay = 0.5f;
    
        public event Action OnContinueClicked;
    
        private Transform currentNpcTransform;
        private Coroutine autoHideCoroutine;
    
        public bool IsTyping
        {
            get
            {
                bool npcTyping = npcBubble != null && npcBubble.gameObject.activeSelf && npcBubble.IsTyping;
                bool playerTyping = playerBubble != null && playerBubble.gameObject.activeSelf && playerBubble.IsTyping;
                return npcTyping || playerTyping;
            }
        }
    
        private void Start()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
    
            Hide();
        }
    
        public void SetCurrentNPC(Transform npc)
        {
            currentNpcTransform = npc;
    
            if (npcBubble != null)
            {
                npcBubble.targetNPC = npc;
                npcBubble.offset = Vector3.up * npcHeightOffset;
            }
        }
    
        public void ShowLine(string text, string speakerName = "")
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
    
            bool isPlayer = !string.IsNullOrEmpty(speakerName) &&
                           speakerName.Equals("Player", StringComparison.OrdinalIgnoreCase);
    
            if (isPlayer)
            {
                ShowPlayerBubble(text);
            }
            else
            {
                ShowNPCBubble(text);
            }
    
            if (waitForTypingToFinish)
            {
                StartCoroutine(WaitForTypingThenNotify());
            }
        }
    
        private void ShowNPCBubble(string text)
        {
            if (playerBubble != null)
            {
                playerBubble.HideDialogue();
            }
    
            if (npcBubble != null && currentNpcTransform != null)
            {
                npcBubble.ShowDialogue(currentNpcTransform, text);
            }
        }
    
        private void ShowPlayerBubble(string text)
        {
            if (npcBubble != null)
            {
                npcBubble.HideDialogue();
            }
    
            if (playerBubble != null && playerTransform != null)
            {
                if (playerBubble.targetNPC != playerTransform)
                {
                    playerBubble.targetNPC = playerTransform;
                    playerBubble.offset = Vector3.up * playerHeightOffset;
                }
                playerBubble.ShowDialogue(playerTransform, text);
            }
        }
    
        private IEnumerator WaitForTypingThenNotify()
        {
            while (IsTyping)
            {
                yield return null;
            }
    
            yield return new WaitForSeconds(autoHideDelay);
    
            OnContinueClicked?.Invoke();
        }
    
        public void Hide()
        {
            if (npcBubble != null)
            {
                npcBubble.HideDialogue();
            }
    
            if (playerBubble != null)
            {
                playerBubble.HideDialogue();
            }
        }
    }