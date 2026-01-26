using UnityEngine;


public class TurtleFishingTutorial : MonoBehaviour
{
    [Header("References")]
    public DialogueNPC oldTurtle;

    [Header("Fishing Settings")]
    public int requiredCatches = 2;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private int fishCaught = 0;
    [SerializeField] private bool tutorialActive = false;

    private void Start()
    {
        if (ConversationManager.Instance != null)
        {
            ConversationManager.Instance.OnDialogueEnd += OnDialogueEnded;
            Log("Subscribed to ConversationManager.OnDialogueEnd");
        }
        else
        {
            Debug.LogError("[TurtleFishingTutorial] ConversationManager not found in scene!");
        }

        
        if (FishingManager.Instance != null)
        {
            FishingManager.Instance.OnFishCaught.AddListener(OnFishCaughtFromManager);
            Log("Subscribed to FishingManager.OnFishCaught");
        }
        else
        {
            Debug.LogError("[TurtleFishingTutorial] FishingManager not found in scene!");
        }
        
        
     
    }

    private void OnDestroy()
    {
       
        if (ConversationManager.Instance != null)
        {
            ConversationManager.Instance.OnDialogueEnd -= OnDialogueEnded;
        }
        
        if (FishingManager.Instance != null)
        {
            FishingManager.Instance.OnFishCaught.RemoveListener(OnFishCaughtFromManager);
        }
    }

  
    private void OnDialogueEnded()
    {
      
        if (GameFlags.Instance.GetFlag("turtle_fishing_complete"))
        {
            Log("Tutorial already complete, skipping start");
         
        }

     
        bool metTurtle = GameFlags.Instance.GetFlag("turtle_met");
        
        if (!metTurtle && !tutorialActive)
        {
            StartFishingTutorial();
        }
    }

   
    private void StartFishingTutorial()
    {
        tutorialActive = true;
        fishCaught = 0;
        
        Log("=== FISHING TUTORIAL STARTED ===");
        Log("Player needs to catch a fish to continue");
       
        GiveBaitPouch();
    }

    
    private void GiveBaitPouch()
    {
        if (FishingManager.Instance != null && FishingManager.Instance.baits != null && FishingManager.Instance.baits.Length > 0)
        {
          
            FishingManager.Instance.baits[0].ResetToFull();
            Log($"Bait refilled! Player has {FishingManager.Instance.baits[0].CurrentDurability} bait");
        }
        else
        {
            Debug.LogWarning("[TurtleFishingTutorial] Could not refill bait - FishingManager or baits not found");
        }
    }

   
    private void OnFishCaughtFromManager(FishScriptiableObject fish)
    {
        if (!tutorialActive)
        {
            return;
        }

        fishCaught++;
        Log($"Fish caught: {fish.fishName} ({fishCaught}/{requiredCatches})");

       
        if (fishCaught >= requiredCatches)
        {
            CompleteFishingTutorial();
        }
        else
        {
            Log($"Need {requiredCatches - fishCaught} more fish");
        }
    }

  
    private void CompleteFishingTutorial()
    {
        tutorialActive = false;
        
        Log("=== FISHING TUTORIAL COMPLETE ===");
        
       
        GameFlags.Instance.SetFlag("turtle_fishing_complete", true);
        
        Log("Flag 'turtle_fishing_complete' set to TRUE");
        
        if (oldTurtle != null)
        {
            oldTurtle.SetActiveConversation("after_fishing");
            Log("Turtle conversation changed to 'after_fishing'");
            Log("Player should return to turtle to see new dialogue!");
        }
        else
        {
            Debug.LogError("[TurtleFishingTutorial] Old Turtle reference is null! Cannot change conversation.");
        }
        
        
    }

  
    public bool IsTutorialActive()
    {
        return tutorialActive;
    }

   
    public int GetFishCaught()
    {
        return fishCaught;
    }

    
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[TurtleFishingTutorial] {message}");
        }
    }
    
    private void OnValidate()
    {
        if (oldTurtle == null)
        {
            Debug.LogWarning("[TurtleFishingTutorial] Old Turtle reference not assigned!");
        }
    }
}