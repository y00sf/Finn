using UnityEngine;

public class JournalPage : MonoBehaviour
{
    [SerializeField] private RectTransform cellsContainer;
    
    private int cellCount = 0;

    public RectTransform CellsContainer => cellsContainer;
    public int CellCount => cellCount;

    public void Initialize()
    {
        cellCount = 0;
    }

    public void AddCell()
    {
        cellCount++;
    }
}