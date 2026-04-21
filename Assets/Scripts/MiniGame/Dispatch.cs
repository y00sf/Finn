using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Dispatch : MiniGameBase, IDifficultyScaler
{
    [Header("Dispatch Settings")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private GameObject[] arrowPrefabs;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;
    
    [SerializeField] private float decaySpeed = 25f;
    [SerializeField] private int maxArrows = 4;
    [SerializeField] private float initialFillAmount = 100f; 

    private Queue<Arrow> currentArrows = new Queue<Arrow>();
    private float difficultyMultiplier = 1f;
    
   
    public InputAction upAction;
    public InputAction downAction;
    public InputAction leftAction;
    public InputAction rightAction;


    protected override void OnStart()
    {
    
        slider.value = initialFillAmount;
        currentArrows.Clear();
      
        foreach(Transform child in panel) Destroy(child.gameObject);

      
        for (int i = 0; i < maxArrows; i++) SpawnArrow();

       
        EnableInputs(true);
    }
    
    protected override void OnCleanup()
    {
        EnableInputs(false);
        foreach(Transform child in panel) Destroy(child.gameObject);
        currentArrows.Clear();
        difficultyMultiplier = 1f;
    }

    private void EnableInputs(bool enable)
    {
        if (enable)
        {
            upAction.Enable(); downAction.Enable(); leftAction.Enable(); rightAction.Enable();
            upAction.performed += _ => CheckInput(ArrowDirection.Up);
            downAction.performed += _ => CheckInput(ArrowDirection.Down);
            leftAction.performed += _ => CheckInput(ArrowDirection.Left);
            rightAction.performed += _ => CheckInput(ArrowDirection.Right);
        }
        else
        {
            upAction.Disable(); downAction.Disable(); leftAction.Disable(); rightAction.Disable();
            upAction.performed -= _ => CheckInput(ArrowDirection.Up);
        }
    }

    void Update()
    {
        if (!_isRunning) return;

        DecreaseSlider();
        UpdateColor();

        
        if (currentArrows.Count <= 0)
        {
            EndGame(true); 
        }
        
        
        if (slider.value <= 0)
        {
            EndGame(false); 
        }
    }
    
    void SpawnArrow() {
        int randomIndex = Random.Range(0, arrowPrefabs.Length);
        GameObject newObj = Instantiate(arrowPrefabs[randomIndex], panel);
        Arrow arrowScript = newObj.GetComponent<Arrow>();
        currentArrows.Enqueue(arrowScript);
    }

    void CheckInput(ArrowDirection inputDir)
    {
        if (currentArrows.Count == 0) return;

        Arrow targetArrow = currentArrows.Peek();

        if (inputDir == targetArrow.direction)
        {
            currentArrows.Dequeue();
            Destroy(targetArrow.gameObject);
        }
        else
        {
          
            slider.value -= 10f; 
        }
    }
    
    private void DecreaseSlider()
    {
        slider.value -= decaySpeed * difficultyMultiplier * Time.deltaTime; 
    }

    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Clamp(multiplier, 0.1f, 5f);
    }
    private void UpdateColor()
    {
        if (fillImage != null) fillImage.color = colorGradient.Evaluate(slider.normalizedValue);
    }
}
