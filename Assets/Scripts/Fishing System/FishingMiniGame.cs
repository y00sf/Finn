using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class FishingMiniGame : MiniGameBase
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform Target;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Input")]
    [SerializeField] private InputAction clickAction;

    [Header("Game Settings")]
    [SerializeField] private float speed = 200f;
    [SerializeField] private float speedIncrease = 50f;
    [SerializeField] private int counterCount = 10;
    [SerializeField] private float hitTolerance = 25f;
    [SerializeField] private int health = 3;

    private float currentSpeed;
    private int currentCounter;
    private int currentHealth;
    private bool isClockwise = true;

    private void Update()
    {
        if (!_isRunning) return;

        float directionMultiplier = isClockwise ? -1f : 1f;
        pointer.Rotate(Vector3.forward * directionMultiplier * currentSpeed * Time.deltaTime);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (_isRunning)
        {
            CheckHit();
        }
    }

    protected override void OnStart()
    {
        ResetGameValues();

        if (gamePanel != null)
            gamePanel.SetActive(true);

       
        if (clickAction != null)
        {
            clickAction.Enable();
            clickAction.performed += OnClick;
        }

        ChangeTargetRot();
        UpdateUI();
    }

    protected override void OnCleanup()
    {
    
        if (clickAction != null)
        {
            clickAction.performed -= OnClick;
            clickAction.Disable();
        }

        if (gamePanel != null)
            gamePanel.SetActive(false);
    }

    private void ResetGameValues()
    {
        currentSpeed = speed;
        currentCounter = counterCount;
        currentHealth = health;
        isClockwise = true;
        pointer.rotation = Quaternion.identity;
    }

    private void CheckHit()
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
}