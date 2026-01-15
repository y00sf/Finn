using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ReactionTime : MonoBehaviour
{
    [SerializeField] private GameObject circlePrefab; 
    [SerializeField] private RectTransform spawnArea; 
    [SerializeField] private float spawnRate = 1.0f;
    [SerializeField] private int maxSpwanCount = 3;
    [SerializeField] private int currentSpawnCount = 0;
    public bool isGameActive = true;
    public UnityEvent OnWin;
    private bool hasWon = false;
    
    public InputAction hitAction; 

    private List<Circle> activeCircles = new List<Circle>();
    private float nextSpawnTime;

    private void OnEnable() => hitAction.Enable();
    private void OnDisable() => hitAction.Disable();


    void Start()
    {
        currentSpawnCount = 0;
    }
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            if (currentSpawnCount < maxSpwanCount)
            {
                SpawnCircleUI();
                nextSpawnTime = Time.time + spawnRate;
            }
            
        }

        if (hitAction.WasPerformedThisFrame())
        {
            CheckForHits();
        }

        activeCircles.RemoveAll(x => x == null);
    }

    private void SpawnCircleUI()
    {
        if (spawnArea == null) return;

        GameObject obj = Instantiate(circlePrefab, spawnArea);
        
        float width = spawnArea.rect.width;
        float height = spawnArea.rect.height;

        float randomX = Random.Range(-width / 2f, width / 2f);
        float randomY = Random.Range(-height / 2f, height / 2f);

        RectTransform circleRect = obj.GetComponent<RectTransform>();
        circleRect.anchoredPosition = new Vector2(randomX, randomY);
        circleRect.localScale = Vector3.one;
        currentSpawnCount++;
        activeCircles.Add(obj.GetComponent<Circle>());
    }

    private void CheckForHits()
    {
        foreach (var circle in activeCircles)
        {
            if (circle.CheckHit()) break; 
        }
    }
}
