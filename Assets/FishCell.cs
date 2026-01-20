using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishCell : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fishIcon;
    //[SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private Button cellButton;

    private FishScriptiableObject fishData;
    private Journal journalReference;

    private void Awake()
    {
        if (cellButton != null)
        {
            cellButton.onClick.AddListener(OnCellClicked);
        }
    }

    public void Setup(FishScriptiableObject fish, Journal journal)
    {
        fishData = fish;
        journalReference = journal;

        if (fish == null) return;

        if (fishIcon != null)
            fishIcon.sprite = fish.SmallFishSprite;

        // if (fishNameText != null)
        //     fishNameText.text = fish.FishName;
    }

    private void OnCellClicked()
    {
        if (fishData != null && journalReference != null)
        {
            journalReference.ShowFishInfo(fishData);
        }
    }

    private void OnDestroy()
    {
        if (cellButton != null)
        {
            cellButton.onClick.RemoveListener(OnCellClicked);
        }
    }
}