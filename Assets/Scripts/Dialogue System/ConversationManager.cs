using System;
using UnityEngine;
using Unity.Cinemachine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [Header("Scripts to Toggle")]
    [SerializeField] private FishingCaster fishingCaster;
    [SerializeField] private PlayerMovement playerInput;
    [SerializeField] private PlayerInteraction playerInteraction;
    [Header("Input Lock")]
    [SerializeField] private float postDialogueInputLock = 0.3f;
    [Header("Auto End Distance")]
    [SerializeField] private bool endDialogueWhenOutOfRange = true;
    [SerializeField] private float outOfRangeBuffer = 0.15f;
    [SerializeField] private float fallbackEndDistance = 3f;
    [Header("Camera Focus")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private float zoomInMultiplier = 0.9f;
    [SerializeField] private float zoomDuration = 0.35f;
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private DialogueData currentDialogue;
    private DialogueNode currentNode;
    private NPC currentNPC;
    private GameObject currentNPCObject;
    private bool isRunning = false;
    private float lastDialogueEndTime = -100f;
    private LensSettings baseLens;
    private bool hasBaseLens;
    private float currentZoom;
    private Coroutine zoomCoroutine;
    private Transform currentRangeRoot;
    private Collider[] currentRangeColliders;
    
    [SerializeField] public event Action<DialogueData> OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action<DialogueNode> OnLineShow;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindObjectOfType<DialogueUI>();

        if (dialogueUI != null)
        {
            dialogueUI.OnContinueClicked += HandleContinue;
            dialogueUI.OnNpcCharacterRevealed += HandleNpcCharacterRevealed;
        }

        CacheBaseLens();
    }

    private void Update()
    {
        if (!isRunning || !endDialogueWhenOutOfRange) return;
        if (!IsPlayerOutOfDialogueRange()) return;

        EndDialogue();
    }
    
    public void StartDialogue(DialogueData dialogue, NPC npc, GameObject NPCGameObject ,Transform npcTransform = null, Transform npcRangeRoot = null)
    {
        if (dialogue == null || !dialogue.Validate()) 
        {
            Debug.LogError("DialogueData is null or invalid!");
            return;
        }

        if (isRunning) return;

        if (dialogue.lines == null || dialogue.lines.Count == 0) return;
    
        currentDialogue = dialogue;
        currentNode = dialogue.lines[0];
        currentNPC = npc;
        isRunning = true;
        currentNPCObject = NPCGameObject;
        currentNPCObject.GetComponent<PathNPC>().SetCanWalk(false);
        
        
        if (dialogueUI != null && npcTransform != null)
        {
            dialogueUI.SetCurrentNPC(npcTransform);
        }

        SetDialogueRangeSource(npcRangeRoot != null ? npcRangeRoot : npcTransform);

        StartCameraZoom(true);
        OnDialogueStart?.Invoke(dialogue);
        SetPlayerScriptsEnabled(false);
        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }
        
        if (currentNode.setFlag && !string.IsNullOrEmpty(currentNode.flagToSet))
        {
            GameFlags.Instance.SetFlag(currentNode.flagToSet, currentNode.flagValue);
        }

        OnLineShow?.Invoke(currentNode);
        
        if (dialogueUI != null)
        {
            dialogueUI.ShowLine(currentNode.text, currentNode.speakerName, currentNPC);
        }
    }

    private void HandleNpcCharacterRevealed(char _)
    {
        if (!isRunning) return;
        if (currentNode == null || currentNPCObject == null) return;
        if (string.Equals(currentNode.speakerName, "Player", StringComparison.OrdinalIgnoreCase)) return;

        if (currentNPCObject.TryGetComponent<DialogueNPC>(out var dialogueNpc))
        {
            dialogueNpc.TalkAnimation();
        }
    }

    private void HandleContinue()
    {
        if (!isRunning || currentNode == null) return;

        string nextID = currentNode.nextNodeID;
        
        if (currentNode.useCondition && !string.IsNullOrEmpty(currentNode.requiredFlag))
        {
            bool flagValue = GameFlags.Instance.GetFlag(currentNode.requiredFlag);
            
            nextID = flagValue ? currentNode.trueNodeID : currentNode.falseNodeID;
        
            Debug.Log($"[Branching] Flag '{currentNode.requiredFlag}' is {flagValue}. Going to: {nextID}");
        }

        if (!string.IsNullOrEmpty(nextID))
        {
            DialogueNode nextNode = currentDialogue.GetNode(nextID);
            if (nextNode != null)
            {
                currentNode = nextNode;
                ShowCurrentNode();
            }
            else
            {
                Debug.LogWarning($"Node '{nextID}' not found. Ending dialogue.");
                EndDialogue();
            }
        }
        else
        {
            Debug.Log("No next node - ending dialogue");
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (!isRunning) return;

        Debug.Log("Ending dialogue...");
    
        isRunning = false;
        currentDialogue = null;
        currentNode = null;
        currentRangeRoot = null;
        currentRangeColliders = null;
        currentNPCObject.GetComponent<PathNPC>().SetCanWalk(true);
        currentNPCObject = null;
        

        if (dialogueUI != null) dialogueUI.Hide();
        StartCameraZoom(false);
        
        StartCoroutine(EnablePlayerAfterDelay());

        lastDialogueEndTime = Time.unscaledTime;
        OnDialogueEnd?.Invoke();
    }
    
    private System.Collections.IEnumerator EnablePlayerAfterDelay()
    {
   
        yield return new WaitForSeconds(0.8f);
    
        SetPlayerScriptsEnabled(true);
    }

    private void OnDestroy()
    {
        if (dialogueUI != null)
        {
            dialogueUI.OnContinueClicked -= HandleContinue;
            dialogueUI.OnNpcCharacterRevealed -= HandleNpcCharacterRevealed;
        }
    }
    
    private void SetPlayerScriptsEnabled(bool enabled)
    {
        if (fishingCaster != null) fishingCaster.enabled = enabled;
        if (playerInteraction != null) playerInteraction.SetInteractionEnabled(enabled);
    }

    private void SetDialogueRangeSource(Transform sourceRoot)
    {
        currentRangeRoot = sourceRoot;
        currentRangeColliders = sourceRoot != null ? sourceRoot.GetComponentsInChildren<Collider>(true) : null;
    }

    private bool IsPlayerOutOfDialogueRange()
    {
        if (currentRangeRoot == null) return false;

        Vector3 playerCheckPosition = playerInteraction != null
            ? playerInteraction.GetInteractionOrigin()
            : (playerInput != null ? playerInput.transform.position : transform.position);

        float allowedDistance = (playerInteraction != null ? playerInteraction.GetInteractionDistance() : fallbackEndDistance)
                                + outOfRangeBuffer;
        float allowedDistanceSqr = allowedDistance * allowedDistance;

        if (currentRangeColliders != null && currentRangeColliders.Length > 0)
        {
            foreach (var col in currentRangeColliders)
            {
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy) continue;

                Vector3 closestPoint = col.ClosestPoint(playerCheckPosition);
                if ((closestPoint - playerCheckPosition).sqrMagnitude <= allowedDistanceSqr)
                {
                    return false;
                }
            }

            return true;
        }

        return (currentRangeRoot.position - playerCheckPosition).sqrMagnitude > (fallbackEndDistance * fallbackEndDistance);
    }

    private void CacheBaseLens()
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineCamera>();
        }

        if (virtualCamera == null) return;

        baseLens = virtualCamera.Lens;
        hasBaseLens = true;
    }

    private void StartCameraZoom(bool zoomIn)
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineCamera>();
            if (virtualCamera == null) return;
        }

        if (zoomIn || !hasBaseLens)
        {
            baseLens = virtualCamera.Lens;
            hasBaseLens = true;
        }

        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomRoutine(zoomIn ? 1f : 0f));
    }

    private System.Collections.IEnumerator ZoomRoutine(float targetZoom)
    {
        if (!hasBaseLens) yield break;

        float startZoom = currentZoom;
        float duration = Mathf.Max(0.001f, zoomDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float eased = zoomCurve != null ? zoomCurve.Evaluate(t) : t;
            currentZoom = Mathf.Lerp(startZoom, targetZoom, eased);
            ApplyZoom();

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentZoom = targetZoom;
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        if (virtualCamera == null || !hasBaseLens) return;

        float zoomFactor = Mathf.Lerp(1f, zoomInMultiplier, currentZoom);
        LensSettings lens = baseLens;

        if (lens.Orthographic)
        {
            lens.OrthographicSize = baseLens.OrthographicSize * zoomFactor;
        }
        else
        {
            lens.FieldOfView = baseLens.FieldOfView * zoomFactor;
        }

        virtualCamera.Lens = lens;
    }
    
    public bool IsDialogueRunning()
    {
        return isRunning;
    }

    public bool IsInputLocked()
    {
        return Time.unscaledTime - lastDialogueEndTime < postDialogueInputLock;
    }
}
