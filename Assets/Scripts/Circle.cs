using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Circle : MonoBehaviour
{
    [SerializeField] private RectTransform mainCircle;
    [SerializeField] private RectTransform approachRing;
    
    [SerializeField] private float startScale = 3f;
    [SerializeField] private float shrinkSpeed = 1f;
    [SerializeField] private float hitTolerance = 0.2f;

    void Start()
    {
        approachRing.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        approachRing.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (approachRing.localScale.x <= mainCircle.localScale.x - 0.1f)
        {
            Destroy(gameObject); 
        }
    }

    public bool CheckHit()
    {
        float approachSize = approachRing.localScale.x;
        float targetSize = mainCircle.localScale.x;
        
        float sizeDifference = Mathf.Abs(approachSize - targetSize);

        if (sizeDifference <= hitTolerance)
        {
            Debug.Log("Hit");
            Destroy(gameObject);
            return true;
        }
        return false; 
    }
}
