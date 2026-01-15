using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Dispatch : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private GameObject[] arrowPrefabs;
    [SerializeField] private Queue<Arrow> currentArrows = new Queue<Arrow>();
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;
    
    [SerializeField] private float decaySpeed = 25f;
    [SerializeField] private int maxArrows = 4;
    [SerializeField] private float fillAmount = 15f;
    
    public InputAction upAction;
    public InputAction downAction;
    public InputAction leftAction;
    public InputAction rightAction;

    private void OnEnable()
    {
        upAction.Enable();
        downAction.Enable();
        leftAction.Enable();
        rightAction.Enable();

        
        upAction.performed += _ => CheckInput(ArrowDirection.Up);
        downAction.performed += _ => CheckInput(ArrowDirection.Down);
        leftAction.performed += _ => CheckInput(ArrowDirection.Left);
        rightAction.performed += _ => CheckInput(ArrowDirection.Right);
    }

    private void OnDisable()
    {
        upAction.Disable();
        downAction.Disable();
        leftAction.Disable();
        rightAction.Disable();
    }

    void Start()
    {
        for (int i = 0; i < maxArrows; i++)
        {
            SpawnArrow();
        }
    }

    void Update()
    {
        DecreaseSlider();
        UpdateColor();
        if (currentArrows.Count <= 0)
        {
            Debug.Log("win");
        }
    }

    void SpawnArrow()
    {
       
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
            Debug.Log("Correct!");
            
          
            currentArrows.Dequeue();
            Destroy(targetArrow.gameObject);
            return;
        }
        else
        {
            Debug.Log("Wrong Input!");
            return;
        }
    }
    
    private void IncreaseSlider()
    {
        slider.value += fillAmount;
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
}