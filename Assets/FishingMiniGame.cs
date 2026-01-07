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
    [Header("Game Setttigns")]
    [SerializeField] private float speed = 200f;
    [SerializeField] private float speedIncrease = 50f;
    [SerializeField] private int counterCount = 10;
    [SerializeField] private float hitTolerance = 25f;
    [SerializeField] private int health = 3;
    [SerializeField] private FishingSpotManager fishingSpotManager;

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
        
    }

    // Update is called once per frame
    void Update()
    {

        if (!isActive)
        {
;         return;
        }
       
        float directionMultiplier = isClockwise ? -1f : 1f;
        
       
        pointer.Rotate(Vector3.forward * directionMultiplier * currentSpeed * Time.deltaTime);

       
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
        if (counterText != null)
        {
            counterText.text = currentCounter.ToString();
        }
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
    }

    private void ChangeTargetRot()
    {
        
        float randomAngle = Random.Range(0f, 360f);
        
       
        Target.rotation = Quaternion.Euler(0, 0, randomAngle);
    }

    private void OnFail()
    {
        currentHealth--;
        UpdateUI();

        if (currentHealth <= 0)
        {
            EndGame(false);
        }
    }

    private void OnSuccess()
    {
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
    
    
    private void EndGame(bool playerWon)
    {
        isActive = false;

        if (gamePanel != null)
        {
            gamePanel.SetActive(false);
        }
          

        if (playerWon)
        {
            NotifecationText.text = "Fish caught";
            fishingSpotManager.SpawnRandomSpot();
            Debug.Log("FISH CAUGHT!");
            
        }
        else
        {
            fishingSpotManager.SpawnRandomSpot();
            NotifecationText.text = "Fish escaped";
        }
        
        if (currentFishingSpot != null)
        {
            fishingSpotManager.currentSpots--;
            currentFishingSpot.DistroyFishingSpot();
            currentFishingSpot = null; 
        }
    }
}
