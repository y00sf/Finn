using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ReactionTime : MiniGameBase
{
  [Header("Reaction Settings")]
    [SerializeField] private GameObject circlePrefab; 
    [SerializeField] private RectTransform spawnArea; 
    [SerializeField] private float spawnRate = 1.0f;
    [SerializeField] private int targetHitCount = 5; // Goal to win
    
    public InputAction hitAction; 

    private List<Circle> activeCircles = new List<Circle>();
    private float nextSpawnTime;
    private int hitsRegistered = 0;

    protected override void OnStart()
    {
        hitsRegistered = 0;
        activeCircles.Clear();
        
        foreach(Transform t in spawnArea) Destroy(t.gameObject);
        
        hitAction.Enable();
        hitAction.performed += OnClick;
    }

    protected override void OnCleanup()
    {
        hitAction.Disable();
        hitAction.performed -= OnClick;
        activeCircles.Clear();
    }

    void Update()
    {
        if (!_isRunning) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnCircleUI();
            nextSpawnTime = Time.time + spawnRate;
        }

        
        activeCircles.RemoveAll(x => x == null);
        
        
        if (hitsRegistered >= targetHitCount)
        {
            EndGame(true);
        }
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        CheckForHits();
    }

    private void SpawnCircleUI()
    {
        if (spawnArea == null) return;

        GameObject obj = Instantiate(circlePrefab, spawnArea);
        RectTransform circleRect = obj.GetComponent<RectTransform>();
        
     
        float width = spawnArea.rect.width;
        float height = spawnArea.rect.height;
        circleRect.anchoredPosition = new Vector2(
            Random.Range(-width / 2f, width / 2f), 
            Random.Range(-height / 2f, height / 2f)
        );
        
        Circle c = obj.GetComponent<Circle>();
        activeCircles.Add(c);
        
     
    }

    private void CheckForHits()
    {
      
        for (int i = activeCircles.Count - 1; i >= 0; i--)
        {
            if (activeCircles[i].CheckHit()) 
            {
                hitsRegistered++;
                break; 
            }
        }
    }
}
