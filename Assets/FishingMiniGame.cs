using TMPro;
using UnityEngine;

public class FishingMiniGame : MonoBehaviour
{
    [SerializeField] private GameObject gamePanel;
    
    [Header("UI Elements")]
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform Target;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI NotifecationText;

    [Header("Game Settings")]
    [SerializeField] private float speed = 200f;
    [SerializeField] private float speedIncrease = 50f;
    [SerializeField] private int counterCount = 10;
    [SerializeField] private float hitTolerance = 25f;
    [SerializeField] private int health = 3;
    [SerializeField] private FishingSpotManager fishingSpotManager;

    [Header("Audio System")]
    [Tooltip("Source for the Success/Fail sounds")]
    [SerializeField] private AudioSource sfxSource; 
    [Tooltip("Source for the looping reel sound")]
    [SerializeField] private AudioSource reelSource; 
    
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;
    [Tooltip("This clip should be a seamless loop")]
    [SerializeField] private AudioClip reelLoopClip;
    
    [Header("Audio Settings")]
    [SerializeField] private Vector2 randomPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private float maxReelPitch = 2.0f; // Cap the speed so it doesn't get too crazy

    private FishingSpot currentFishingSpot;
    
    private float currentSpeed;
    private int currentCounter;
    private int currentHealth;
    private bool isClockwise = true;
    private bool isActive;
    
    void Start()
    {
        UpdateUI();
        ChangeTargetRot();
        
        // Safety check to ensure reel loops
        if(reelSource != null) reelSource.loop = true;
    }

    void Update()
    {
        if (!isActive) return;
       
        float directionMultiplier = isClockwise ? -1f : 1f;
        
        // Rotate Pointer
        pointer.Rotate(Vector3.forward * directionMultiplier * currentSpeed * Time.deltaTime);

        // --- UPDATE REEL PITCH ---
        // As currentSpeed increases, pitch increases. 
        // Example: If speed is 200 (base) and current is 300, pitch becomes 1.5x
        if (reelSource != null && reelSource.isPlaying)
        {
            float pitchRatio = currentSpeed / speed; 
            reelSource.pitch = Mathf.Clamp(pitchRatio, 1f, maxReelPitch);
        }

        if (Input.GetMouseButtonDown(0)) 
        {
            CheckHit();
        }
    }

    public void StartFishing(FishingSpot spot)
    {
        currentFishingSpot = spot;
        ResetGameValues();
        
        if (gamePanel != null)
            gamePanel.SetActive(true);
            
        isActive = true;
        
        ChangeTargetRot();
        UpdateUI();

        // Start the Reel Sound
        if (reelSource != null && reelLoopClip != null)
        {
            reelSource.clip = reelLoopClip;
            reelSource.pitch = 1f; // Reset pitch to normal
            reelSource.Play();
        }
    }
    
    private void ResetGameValues()
    {
        currentSpeed = speed;
        currentCounter = counterCount;
        currentHealth = health;
        isClockwise = true;
        pointer.rotation = Quaternion.identity; 
    }

    void CheckHit()
    {
        float pointerAngle = pointer.eulerAngles.z;
        float targetAngle = Target.eulerAngles.z;
    
        float angleDifference = Mathf.DeltaAngle(pointerAngle, targetAngle);

        if (Mathf.Abs(angleDifference) <= hitTolerance)
        {
            OnSuccess();
        }
        else
        {
            OnFail();
        }
    }

    private void UpdateUI()
    {
        if (counterText != null) counterText.text = currentCounter.ToString();
        if (healthText != null) healthText.text = currentHealth.ToString();
    }

    private void ChangeTargetRot()
    {
        float randomAngle = Random.Range(0f, 360f);
        Target.rotation = Quaternion.Euler(0, 0, randomAngle);
    }

    private void OnFail()
    {
        // Play Fail Sound with Random Pitch
        PlayRandomPitchSFX(failClip);

        currentHealth--;
        UpdateUI();

        if (currentHealth <= 0)
        {
            EndGame(false);
        }
    }

    private void OnSuccess()
    {
        // Play Success Sound with Random Pitch
        PlayRandomPitchSFX(successClip);

        currentCounter--;
        UpdateUI();

        if (currentCounter <= 0)
        {
            EndGame(true);
            return;
        }
        isClockwise = !isClockwise;
        currentSpeed += speedIncrease;
        
        ChangeTargetRot();
    }
    
    // Helper function to keep code clean
    private void PlayRandomPitchSFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = Random.Range(randomPitchRange.x, randomPitchRange.y);
            sfxSource.PlayOneShot(clip);
        }
    }
    
    private void EndGame(bool playerWon)
    {
        isActive = false;

        // Stop the reel sound immediately
        if (reelSource != null)
        {
            reelSource.Stop();
        }

        if (gamePanel != null)
        {
            gamePanel.SetActive(false);
        }
        
        if (currentFishingSpot != null)
        {
            fishingSpotManager.CurrentSpots--;
            currentFishingSpot.DistroyFishingSpot();
            currentFishingSpot = null; 
        }
          
        fishingSpotManager.SpawnRandomSpot();
        if (playerWon)
        {
            NotifecationText.text = "Fish caught";
            Debug.Log("FISH CAUGHT!");
        }
        else
        {
            NotifecationText.text = "Fish escaped";
        }
    }
}