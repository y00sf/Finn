using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

    [Header("Prefab Reference")] 
    public GameObject baitPrefab;

    public int CurrentDurability
    {
        get => currentDurability;
        set => currentDurability = Mathf.Clamp(value, 0, maxDurability);
    }
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

    [SerializeField] private InputAction GoRight;
    [SerializeField] private InputAction GoLeft;

    [Header("Fish Database")]
    [SerializeField] private FishScriptiableObject[] iceBiomeFish;
    [SerializeField] private FishScriptiableObject[] volcanoBiomeFish;
    [SerializeField] private FishScriptiableObject[] wildeBiomeFish;

    [Header("UI References")]
    [SerializeField] private GameObject catchNotificationPanel;
    [SerializeField] private TextMeshProUGUI catchText;
    [SerializeField] private Image catchIcon;
    [SerializeField] private Image BaitImage;
    [SerializeField] private float notificationDuration = 3f;

    [Header("Bait Inventory")]
    [SerializeField] public BaitItem[] baits;
    
    [SerializeField] public int currentBaitIndex = 0;
    [SerializeField] public int durabilityCostPerCast = 1;

    [Header("Bait Durability UI")]
    [SerializeField] private Slider durabilitySlider;
    [SerializeField] private Image durabilityFillImage;
    [SerializeField] private Gradient durabilityGradient;

    [Header("Global MiniGames")] 
    [SerializeField] private FishingMiniGame finalReelingGame; 

    [Header("Events")]
    public UnityEvent<FishScriptiableObject> OnFishCaught;
    public UnityEvent OnFishEscaped;

    [SerializeField] private BiomeType currentBiome = BiomeType.WiledBiome;
    private FishScriptiableObject currentFishData;
    private IMiniGame _activeMiniGame;

    private void OnDestroy()
    {
        Debug.LogWarning($"[FishingManager] The FishingManager on object '{gameObject.name}' was destroyed! Stack Trace: {System.Environment.StackTrace}");
    }

    private void OnEnable()
    {
        GoLeft?.Enable();
        GoRight?.Enable();

        GoLeft.performed += OnGoLeft;
        GoRight.performed += OnGoRight;
    }

    private void OnDisable()
    {
        GoLeft.performed -= OnGoLeft;
        GoRight.performed -= OnGoRight;
        
        GoLeft?.Disable();
        GoRight?.Disable();
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

        ResetAllFishData();
    }

    private void Start()
    {
        if (catchNotificationPanel != null)
            catchNotificationPanel.SetActive(false);
        
        if (finalReelingGame != null) 
            finalReelingGame.gameObject.SetActive(false);

        if (baits != null)
        {
            foreach (var b in baits)
            {
                if (b != null && b.MiniGame != null)
                {
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

    public void RefillAllBaits()
    {
        if (baits == null) return;
        
        foreach (var bait in baits)
        {
            if (bait != null)
            {
                bait.ResetToFull();
            }
        }
        
        UpdateDurabilityUI();
    }

    public void SetCurrentBait(int index)
    {
        if (baits == null || baits.Length == 0) return;
        if (index < 0 || index >= baits.Length) return;

        currentBaitIndex = index;
        BaitImage.sprite = baits[index].icon;
        UpdateDurabilityUI();
    }

    public void OnBaitLanded()
    {
        if (baits == null || baits.Length == 0) return;
        var bait = baits[currentBaitIndex];
        if (bait == null) return;
        
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
            if (_activeMiniGame == finalReelingGame)
            {
                if (currentFishData != null)
                    ProcessCaughtFish(currentFishData);

                _activeMiniGame = null;
            }
            else 
            {
                if (finalReelingGame != null)
                {
                    LaunchMiniGameForBait(finalReelingGame);
                }
                else
                {
                    Debug.LogWarning("[FishingManager] No Final Reeling Game assigned! Catching immediately.");
                    ProcessCaughtFish(currentFishData);
                    _activeMiniGame = null;
                }
            }
        }
        else
        {
            OnFishEscaped?.Invoke();
            _activeMiniGame = null;
        }
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
        BaitImage.sprite = baits[currentBaitIndex].icon;
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
        fish.collected = true; 
        OnFishCaught?.Invoke(fish);
        ShowCatchNotification(fish);
        Debug.Log($"[FishingManager] Caught {fish.FishName}. Marked collected as: {fish.collected}");
    }

    private void ShowCatchNotification(FishScriptiableObject fish)
    {
        if (catchNotificationPanel == null) return;
        catchNotificationPanel.SetActive(true);
        if (catchText != null) catchText.text = fish.FishName;
        if (catchIcon != null) catchIcon.sprite = fish.LargeFishSprite;
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
    
    public System.Collections.Generic.List<FishScriptiableObject> GetAllFish()
    {
        var allFish = new System.Collections.Generic.List<FishScriptiableObject>();
        if (iceBiomeFish != null) allFish.AddRange(iceBiomeFish);
        if (volcanoBiomeFish != null) allFish.AddRange(volcanoBiomeFish);
        if (wildeBiomeFish != null) allFish.AddRange(wildeBiomeFish);
        return allFish;
    }
    
    private void ResetAllFishData()
    {
        var allFish = new System.Collections.Generic.List<FishScriptiableObject>();
        if (iceBiomeFish != null) allFish.AddRange(iceBiomeFish);
        if (volcanoBiomeFish != null) allFish.AddRange(volcanoBiomeFish);
        if (wildeBiomeFish != null) allFish.AddRange(wildeBiomeFish);

        foreach (var fish in allFish)
        {
            if (fish != null)
            {
                fish.collected = false; 
            }
        }
    }

    private void OnGoLeft(InputAction.CallbackContext context)
    {
        currentBaitIndex--;
        if(currentBaitIndex < 0){currentBaitIndex = baits.Length -1;}
        SetCurrentBait(currentBaitIndex);
    }
    private void OnGoRight(InputAction.CallbackContext context)
    {
        currentBaitIndex++;
        if(currentBaitIndex > baits.Length -1){currentBaitIndex = 0;}
        SetCurrentBait(currentBaitIndex);
    }
    
}