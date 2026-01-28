using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FishingMiniGame : MiniGameBase
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform Target;
    [SerializeField] private Image TargetImage;
    [SerializeField] private TextMeshProUGUI counterText;
    //[SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartContainer;

    [SerializeField] private float startDelay = 1.0f; 
    
    [SerializeField] private InputAction clickAction;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float speed = 200f;
    [SerializeField] private float speedIncrease = 50f;
    [SerializeField] private int counterCount = 10;
    [SerializeField] private float hitTolerance = 25f;
    [SerializeField] private int health = 3;

    private float currentSpeed;
    private int currentCounter;
    private int currentHealth;
    private bool isClockwise = true;
    
    private bool isInputActive = false; 
    private List<GameObject> activeHearts = new List<GameObject>();

    private void Update()
    {
        if (!_isRunning || !isInputActive) return;

        float directionMultiplier = isClockwise ? -1f : 1f;
        pointer.Rotate(Vector3.forward * directionMultiplier * currentSpeed * Time.deltaTime);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (_isRunning && isInputActive)
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

        if (audioSource == null) 
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        SetupHearts(currentHealth);
        ChangeTargetRot();
        UpdateUI();
        StartCoroutine(StartGameDelay());
    }

    private IEnumerator StartGameDelay()
    {
        isInputActive = false;
        yield return new WaitForSeconds(startDelay);
        isInputActive = true;
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
            
        StopAllCoroutines();
    }

    private void ResetGameValues()
    {
        currentSpeed = speed;
        currentCounter = counterCount;
        currentHealth = health;
        isClockwise = true;
        pointer.rotation = Quaternion.identity;
        isInputActive = false;
    }

    private void SetupHearts(int count)
    {
        foreach(Transform child in heartContainer) Destroy(child.gameObject);
        activeHearts.Clear();
        for(int i = 0; i < count; i++) 
            activeHearts.Add(Instantiate(heartPrefab, heartContainer));
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
       /* if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
        */
    }

    private void ChangeTargetRot()
    {
        float randomAngle = Random.Range(0f, 360f);
        Target.rotation = Quaternion.Euler(0, 0, randomAngle);
    }

    private void ChangeTargetSize()
    {
        float size = Random.Range(0.06f, 0.12f);
        TargetImage.fillAmount = size;
    }

    private void OnFail()
    {
        currentHealth--;
        
        if(activeHearts.Count > 0)
        {
            GameObject h = activeHearts[activeHearts.Count - 1];
            activeHearts.Remove(h);
            Destroy(h);
        }

        PlaySound(failClip);
        UpdateUI();

        if (currentHealth <= 0)
        {
            EndGame(false);
        }
    }

    private void OnSuccess()
    {
        currentCounter--;
        PlaySound(successClip);
        UpdateUI();

        if (currentCounter <= 0)
        {
            EndGame(true);
            return;
        }

        isClockwise = !isClockwise;
        currentSpeed += speedIncrease;

        ChangeTargetSize();
        ChangeTargetRot();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) 
            audioSource.PlayOneShot(clip);
    }
}