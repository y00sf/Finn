using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class bait
{
    public string name;
    [TextArea]
    public string description;
    public Sprite icon;
    public int durability;
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Data")]
    public bait[] baits; 
    
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;
    
    private void Start()
    {
       
        if (baits != null && baits.Length > 0)
        {
            InitializeSlider(baits[0].durability);
            Debug.Log($"Slider initialized with max durability: {baits[0].durability}");
        }
        else
        {
            Debug.LogWarning("No baits assigned in PlayerInventory!");
        }
    }
    
    public void InitializeSlider(int maxDurability)
    {
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = maxDurability;
            slider.value = maxDurability; 
            UpdateColor(maxDurability);
            
            Debug.Log($"Slider setup - Min: {slider.minValue}, Max: {slider.maxValue}, Current: {slider.value}");
        }
        else
        {
            Debug.LogError("Slider is not assigned in the Inspector!");
        }
    }
    
    public void UseBait(int amount, int index)
    {
        Debug.Log($"===== UseBait CALLED - Amount: {amount}, Index: {index} =====");
        
        if (baits == null || baits.Length == 0 || index >= baits.Length || index < 0)
        {
            Debug.LogWarning($"Invalid bait usage - Index: {index}, Baits Length: {baits?.Length ?? 0}");
            return;
        }
        
        int oldDurability = baits[index].durability;
        baits[index].durability -= amount;
        
        if (baits[index].durability < 0) 
            baits[index].durability = 0;
       
        Debug.Log($"Bait used! Durability: {oldDurability} -> {baits[index].durability}");
        
        UpdateUI(baits[index].durability);
    }
    
    private void UpdateUI(int currentValue)
    {
        if (slider != null) 
        {
            slider.value = currentValue;
            Debug.Log($"Slider updated to: {currentValue} (Max: {slider.maxValue})");
        }
        else
        {
            Debug.LogError("Slider is null in UpdateUI!");
        }
        
        UpdateColor(currentValue);
    }
    
    private void UpdateColor(int currentValue)
    {
        if (fillImage != null && slider != null)
        {
            float normalizedValue = (float)currentValue / slider.maxValue;
            fillImage.color = colorGradient.Evaluate(normalizedValue);
        }
    }
}