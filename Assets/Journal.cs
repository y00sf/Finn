using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] private GameObject fishCellPrefab;
    
    [Header("Page Container")]
    [SerializeField] private Transform pagesContainer;
    [SerializeField] private int maxCellsPerPage = 6;

    [Header("Info Page")]
    [SerializeField] private GameObject infoPage;
    [SerializeField] private Image fishImageLarge;
    [SerializeField] private TextMeshProUGUI fishName;
    [SerializeField] private TextMeshProUGUI fishDescription;
    [SerializeField] private TextMeshProUGUI fishWeight;
    [SerializeField] private TextMeshProUGUI fishLength;
    [SerializeField] private TextMeshProUGUI fishBiome;

    [Header("Navigation")]
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private TextMeshProUGUI pageNumberText;

    [Header("Notifications")]
    [SerializeField] private GameObject journalNotification;
    [SerializeField] private float notificationDuration = 3f;

    private HashSet<FishScriptiableObject> caughtFish = new HashSet<FishScriptiableObject>();
    private List<JournalPage> pages = new List<JournalPage>();
    private int currentPageIndex = 0;
    private Coroutine notificationCoroutine;

    private void Start()
    {
        if (FishingManager.Instance != null)
        {
            FishingManager.Instance.OnFishCaught.AddListener(RegisterCaughtFish);
            
            SyncAlreadyCaughtFish(); 
        }

        if (infoPage != null)
            infoPage.SetActive(false);

        if (journalNotification != null)
            journalNotification.SetActive(false);

        CreateInitialPage();
        UpdateNavigationButtons();
    }
    
    private void SyncAlreadyCaughtFish()
    {
        if (FishingManager.Instance == null) return;
        
        List<FishScriptiableObject> allFish = FishingManager.Instance.GetAllFish();

        foreach (var fish in allFish)
        {
            if (fish.collected && !caughtFish.Contains(fish))
            {
                caughtFish.Add(fish);
                CreateFishCell(fish);
            }
        }
    }

    private void OnDestroy()
    {
        if (FishingManager.Instance != null)
        {
            FishingManager.Instance.OnFishCaught.RemoveListener(RegisterCaughtFish);
        }
    }

    private void CreateInitialPage()
    {
        if (pages.Count == 0)
        {
            CreateNewPage();
        }
    }

    private JournalPage CreateNewPage()
    {
        if (pagePrefab == null || pagesContainer == null)
        {
            Debug.LogError("[Journal] Page Prefab or Pages Container not assigned!");
            return null;
        }

        GameObject pageObj = Instantiate(pagePrefab, pagesContainer);
        JournalPage page = pageObj.GetComponent<JournalPage>();
        
        if (page == null)
        {
            Debug.LogError("[Journal] Page Prefab is missing JournalPage component!");
            Destroy(pageObj);
            return null;
        }

        page.Initialize();
        pages.Add(page);
        
        pageObj.SetActive(pages.Count == 1);
        
        return page;
    }

    public void RegisterCaughtFish(FishScriptiableObject fish)
    {
        if (fish == null) return;

        bool isNewFish = !caughtFish.Contains(fish);
        
        if (isNewFish)
        {
            caughtFish.Add(fish);
            CreateFishCell(fish);
            ShowJournalNotification();
            Debug.Log($"[Journal] New fish registered: {fish.FishName}");
        }
    }

    private void CreateFishCell(FishScriptiableObject fish)
    {
        if (fishCellPrefab == null)
        {
            Debug.LogError("[Journal] Fish Cell Prefab is not assigned!");
            return;
        }

        JournalPage targetPage = GetPageWithSpace();
        
        if (targetPage == null)
        {
            targetPage = CreateNewPage();
        }

        if (targetPage == null) return;

        GameObject cell = Instantiate(fishCellPrefab, targetPage.CellsContainer);
        
        FishCell cellScript = cell.GetComponentInChildren<FishCell>();
        if (cellScript != null)
        {
            cellScript.Setup(fish, this);
        }
        else
        {
            Debug.LogError("[Journal] Fish Cell Prefab is missing FishCell script!");
        }

        targetPage.AddCell();
        
        ShowPage(currentPageIndex);
    }

    private JournalPage GetPageWithSpace()
    {
        foreach (var page in pages)
        {
            if (page.CellCount < maxCellsPerPage)
                return page;
        }
        
        return null;
    }

    private void ShowPage(int index)
    {
        if (pages.Count == 0) return;

        currentPageIndex = Mathf.Clamp(index, 0, pages.Count - 1);

        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].gameObject.SetActive(i == currentPageIndex);
            }
        }

        if (pageNumberText != null)
        {
            pageNumberText.text = $"Page {currentPageIndex + 1} / {pages.Count}";
        }

        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        if (previousPageButton != null)
            previousPageButton.interactable = currentPageIndex > 0;

        if (nextPageButton != null)
            nextPageButton.interactable = currentPageIndex < pages.Count - 1 && pages.Count > 1;
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Count - 1)
        {
            ShowPage(currentPageIndex + 1);
        }
    }

    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            ShowPage(currentPageIndex - 1);
        }
    }

    public void ShowFishInfo(FishScriptiableObject fish)
    {
        if (fish == null || infoPage == null) return;

        infoPage.SetActive(true);

        if (fishImageLarge != null)
            fishImageLarge.sprite = fish.LargeFishSprite;

        if (fishName != null)
            fishName.text = fish.FishName;

        if (fishDescription != null)
            fishDescription.text = fish.FishDescription;

        if (fishWeight != null)
            fishWeight.text = $"Weight: {fish.FishWeight}";

        if (fishLength != null)
            fishLength.text = $"Length: {fish.FishLength}";

        if (fishBiome != null)
            fishBiome.text = $"Biome: {fish.BiomeType}";
    }

    public void CloseJournal()
    {
        gameObject.SetActive(false);
    }

    private void ShowJournalNotification()
    {
        if (journalNotification == null || !gameObject.activeInHierarchy) return;

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine = StartCoroutine(ShowNotificationRoutine());
    }

    private System.Collections.IEnumerator ShowNotificationRoutine()
    {
        journalNotification.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        journalNotification.SetActive(false);
        notificationCoroutine = null;
    }

    public int GetTotalFishCaught() => caughtFish.Count;

    public bool HasCaughtFish(FishScriptiableObject fish) => caughtFish.Contains(fish);
}