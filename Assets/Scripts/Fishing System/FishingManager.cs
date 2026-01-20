using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class BaitItem
{
    public string name;
    [TextArea] public string description;
    public Sprite icon;

    [Min(0)] public int maxDurability = 5;
    [SerializeField] private int currentDurability;

    [Header("Minigame Reference")]
  
    [SerializeField] private MiniGameBase miniGame; 

    public int CurrentDurability => currentDurability;
    public MiniGameBase MiniGame => miniGame;

    public void ResetToFull() => currentDurability = Mathf.Clamp(maxDurability, 0, int.MaxValue);

    public bool Consume(int amount)
    {
        if (currentDurability <= 0) return false;
        currentDurability = Mathf.Clamp(currentDurability - amount, 0, maxDurability);
        return true;
    }
}

public class FishingManager : MonoBehaviour
{
    public static FishingManager Instance { get; private set; }

    [Header("Fish Database")]
    [SerializeField] private FishScriptiableObject[] iceBiomeFish;
    [SerializeField] private FishScriptiableObject[] volcanoBiomeFish;
    [SerializeField] private FishScriptiableObject[] wildeBiomeFish;

    [Header("UI References")]
    [SerializeField] private GameObject catchNotificationPanel;
    [SerializeField] private TextMeshProUGUI catchText;
    [SerializeField] private float notificationDuration = 3f;

    [Header("Bait Inventory")]
    [SerializeField] private BaitItem[] baits;
    [SerializeField] private int currentBaitIndex = 0;
    [SerializeField] private int durabilityCostPerCast = 1;

    [Header("Bait Durability UI")]
    [SerializeField] private Slider durabilitySlider;
    [SerializeField] private Image durabilityFillImage;
    [SerializeField] private Gradient durabilityGradient;

    [Header("Events")]
    public UnityEvent<FishScriptiableObject> OnFishCaught;
    public UnityEvent OnFishEscaped;

    [SerializeField] private BiomeType currentBiome = BiomeType.WiledBiome;
    private FishScriptiableObject currentFishData;
    private IMiniGame _activeMiniGame;

 
    private void OnDestroy()
    {
        // If this prints when you didn't expect it, we found the bug.
        Debug.LogWarning($"[FishingManager] The FishingManager on object '{gameObject.name}' was destroyed! Stack Trace: {System.Environment.StackTrace}");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[FishingManager] Duplicate found on '{gameObject.name}'. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (baits != null)
        {
            foreach (var b in baits)
                b?.ResetToFull();
        }
    }

    private void Start()
    {
        if (catchNotificationPanel != null)
            catchNotificationPanel.SetActive(false);

        // Hide all minigame objects at start
        if (baits != null)
        {
            foreach (var b in baits)
            {
                if (b != null && b.MiniGame != null)
                {
                    // SAFETY CHECK 1: Ensure we aren't hiding the Manager or Canvas
                    if (b.MiniGame.gameObject == this.gameObject || b.MiniGame.transform.root == this.transform)
                    {
                        Debug.LogError($"[CRITICAL ERROR] Bait '{b.name}' has the FishingManager or Canvas assigned as its MiniGame! This will delete your UI.");
                        continue;
                    }
                    
                    b.MiniGame.gameObject.SetActive(false);
                }
            }
        }

        UpdateDurabilityUI();
    }

    public void SetCurrentBait(int index)
    {
        if (baits == null || baits.Length == 0) return;
        if (index < 0 || index >= baits.Length) return;

        currentBaitIndex = index;
        UpdateDurabilityUI();
    }

    public void OnBaitLanded()
    {
        if (baits == null || baits.Length == 0) return;
        var bait = baits[currentBaitIndex];
        if (bait == null) return;

        // Consume durability
        if (!bait.Consume(durabilityCostPerCast))
        {
            Debug.Log("[FishingManager] Not enough durability.");
            UpdateDurabilityUI();
            return;
        }

        UpdateDurabilityUI();

        currentFishData = SelectRandomFish();
        if (currentFishData == null) return;

        if (bait.MiniGame == null)
        {
            Debug.LogError($"[FishingManager] Bait '{bait.name}' has NO minigame assigned!");
            return;
        }

        LaunchMiniGameForBait(bait.MiniGame);
    }

    private void LaunchMiniGameForBait(IMiniGame game)
    {
     
        MonoBehaviour gameMono = game as MonoBehaviour;
        if (gameMono != null)
        {
            if (gameMono.gameObject == this.gameObject)
            {
                Debug.LogError("STOP! You assigned the FishingManager object as the MiniGame. I stopped the code from destroying your game.");
                return;
            }
            if (gameMono.gameObject == catchNotificationPanel)
            {
                Debug.LogError("STOP! You assigned the Notification Panel as the MiniGame.");
                return;
            }
            
         
            Transform checkParent = gameMono.transform;
            while (checkParent != null)
            {
                if (checkParent == this.transform || checkParent.GetComponent<Canvas>() != null)
                {
                    Debug.LogError($"CRITICAL ERROR! Bait's MiniGame is assigned to Canvas or FishingManager's parent hierarchy! Object: {gameMono.gameObject.name}");
                    return;
                }
                checkParent = checkParent.parent;
            }
        }

     
        if (_activeMiniGame != null && _activeMiniGame != game)
        {
            MonoBehaviour oldMono = _activeMiniGame as MonoBehaviour;
            if (oldMono != null)
            {
                oldMono.gameObject.SetActive(false);
            }
            _activeMiniGame.ForceCleanup();
        }

        _activeMiniGame = game;
        
    
        if (gameMono != null) 
        {
            gameMono.gameObject.SetActive(true);
        }
        game.BeginGame(OnMiniGameEnded);
    }

    private void OnMiniGameEnded(bool win)
    {
      
        if (_activeMiniGame != null)
        {
            MonoBehaviour gameMono = _activeMiniGame as MonoBehaviour;
            if (gameMono != null)
            {
                gameMono.gameObject.SetActive(false);
            }
        }

        if (win)
        {
            if (currentFishData != null)
                ProcessCaughtFish(currentFishData);
        }
        else
        {
            OnFishEscaped?.Invoke();
        }

        currentFishData = null;
        _activeMiniGame = null;
    }

    private void UpdateDurabilityUI()
    {
        if (baits == null || baits.Length == 0) return;
        int safeIndex = Mathf.Clamp(currentBaitIndex, 0, baits.Length - 1);
        var bait = baits[safeIndex];
        if (bait == null) return;

        if (durabilitySlider != null)
        {
            durabilitySlider.minValue = 0;
            durabilitySlider.maxValue = bait.maxDurability;
            durabilitySlider.SetValueWithoutNotify(bait.CurrentDurability);
        }

        if (durabilityFillImage != null && durabilitySlider != null)
        {
            float t = durabilitySlider.maxValue <= 0 ? 0 : durabilitySlider.value / durabilitySlider.maxValue;
            durabilityFillImage.color = durabilityGradient.Evaluate(t);
        }
    }

    public void SetCurrentBiome(BiomeType biome) => currentBiome = biome;
    private FishScriptiableObject SelectRandomFish()
    {
        var pool = GetFishPoolForBiome(currentBiome);
        return (pool != null && pool.Length > 0) ? pool[Random.Range(0, pool.Length)] : null;
    }
    private FishScriptiableObject[] GetFishPoolForBiome(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.IceBiome => iceBiomeFish,
            BiomeType.VolcanoBiome => volcanoBiomeFish,
            BiomeType.WiledBiome => wildeBiomeFish,
            _ => wildeBiomeFish
        };
    }
    private void ProcessCaughtFish(FishScriptiableObject fish)
    {
        OnFishCaught?.Invoke(fish);
        ShowCatchNotification(fish);
    }
    private void ShowCatchNotification(FishScriptiableObject fish)
    {
        if (catchNotificationPanel == null) return;
        catchNotificationPanel.SetActive(true);
        if (catchText != null) catchText.text = $"Caught: {fish.FishName}!";
        StartCoroutine(HideNotificationAfterDelay());
    }
    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        if (catchNotificationPanel != null) catchNotificationPanel.SetActive(false);
    }
    public bool CanCastWithCurrentBait() => TryGetCurrentBait(out var bait) && bait.CurrentDurability > 0;
    public int GetCurrentBaitDurability() => TryGetCurrentBait(out var bait) ? bait.CurrentDurability : 0;
    public int GetCurrentBaitMaxDurability() => TryGetCurrentBait(out var bait) ? bait.maxDurability : 0;
    private bool TryGetCurrentBait(out BaitItem bait)
    {
        bait = null;
        if (baits == null || baits.Length == 0) return false;
        if (currentBaitIndex < 0 || currentBaitIndex >= baits.Length) return false;
        bait = baits[currentBaitIndex];
        return bait != null;
    }
}