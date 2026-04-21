using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class ButtonMashing : MiniGameBase, IDifficultyScaler
{
    [Header("Mashing Settings")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;
    
  
    public InputAction mashAction; 
    
    [SerializeField] private float increaseAmount = 10f;
    [SerializeField] private float decaySpeed = 25f;
    private float difficultyMultiplier = 1f;

    protected override void OnStart()
    {
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 50;
        mashAction.Enable();
        mashAction.performed += OnMash;
        
        UpdateColor();
    }

    protected override void OnCleanup()
    {
        mashAction.Disable();
        mashAction.performed -= OnMash;
        difficultyMultiplier = 1f;
    }

    private void OnMash(InputAction.CallbackContext context)
    {
        if (_isRunning) 
        {
            IncreaseSlider();
        }
    }

    void Update()
    {
        if (!_isRunning) return;
        CheckConditions();
        DecreaseSlider();
        UpdateColor();
        
    }

    private void IncreaseSlider() => slider.value += increaseAmount;
    private void DecreaseSlider() => slider.value -= decaySpeed * difficultyMultiplier * Time.deltaTime;

    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Clamp(multiplier, 0.1f, 5f);
    }

    private void UpdateColor()
    {
        if (fillImage != null) fillImage.color = colorGradient.Evaluate(slider.normalizedValue);
    }

    private void CheckConditions()
    {
        if (slider.value >= slider.maxValue)
        {
            EndGame(true);
        }
        else if (slider.value <= slider.minValue)
        {
            EndGame(false);
        }
    }
}
