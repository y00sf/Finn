using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class ButtonMashing : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;
    
    public InputAction mashAction;
    [SerializeField] private float increaseAmount = 10f;
    [SerializeField] private float decaySpeed = 25f;
    
    public bool isGameActive = true;
    public UnityEvent OnWin;

    private bool hasWon = false;

    void Start()
    {
        slider.value = 50;
        hasWon = false;
        UpdateColor();
    }

    void Update()
    {
       
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("KEYBOARD 'E' DETECTED!"); 
            IncreaseSlider();
        }

       
        if (UnityEngine.InputSystem.Gamepad.current != null && 
            UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("GAMEPAD 'A/X' DETECTED!");
            IncreaseSlider();
        }
        
        DecreaseSlider();
        UpdateColor();
        CheckWinCondition();
    }

    private void IncreaseSlider()
    {
        slider.value += increaseAmount;
    }

    private void DecreaseSlider()
    {
        slider.value -= decaySpeed * Time.deltaTime;
    }

    private void UpdateColor()
    {
        if (fillImage != null)
        {
            fillImage.color = colorGradient.Evaluate(slider.normalizedValue);
        }
    }

    private void CheckWinCondition()
    {
        if (slider.value >= slider.maxValue)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        hasWon = true;
        OnWin?.Invoke();
    }
}