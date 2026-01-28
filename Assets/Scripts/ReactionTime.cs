using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReactionTime : MiniGameBase
{
    [Header("Reaction Settings")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private float spawnRate = 1.2f;
    [SerializeField] private int targetHitCount = 5;

    public InputAction hitAction;

    private List<Circle> activeCircles = new List<Circle>();
    private float nextSpawnTime;
    private int hitsRegistered = 0;

    [Header("Audio")]
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;
    [SerializeField] private AudioSource audioSource;

    protected override void OnStart()
    {
        hitsRegistered = 0;
        activeCircles.Clear();

        foreach (Transform t in spawnArea) Destroy(t.gameObject);

        hitAction.Enable();
        hitAction.performed += OnClick;
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }
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
        ProcessInput();
    }

    private void SpawnCircleUI()
    {
        if (spawnArea == null) return;

        GameObject obj = Instantiate(circlePrefab, spawnArea);
        RectTransform circleRect = obj.GetComponent<RectTransform>();

        float width = spawnArea.rect.width * 0.8f;
        float height = spawnArea.rect.height * 0.8f;
        
        circleRect.anchoredPosition = new Vector2(
            Random.Range(-width / 2f, width / 2f),
            Random.Range(-height / 2f, height / 2f)
        );

        activeCircles.Add(obj.GetComponent<Circle>());
    }

    private void ProcessInput()
    {
        if (activeCircles.Count == 0) return;

        Circle bestCandidate = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < activeCircles.Count; i++)
        {
            if (activeCircles[i] == null) continue;
            
            float dist = activeCircles[i].GetHitDistance();
            if (dist < minDistance)
            {
                minDistance = dist;
                bestCandidate = activeCircles[i];
            }
        }

        if (bestCandidate != null)
        {
            if (bestCandidate.CheckHit())
            {
                hitsRegistered++;
                activeCircles.Remove(bestCandidate);
                PlayClip(successClip);
            }
            else
            {
                PlayClip(failClip);
            }
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}