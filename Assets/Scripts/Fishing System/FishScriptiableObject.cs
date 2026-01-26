using UnityEngine;

public enum BiomeType
{
    IceBiome,
    VolcanoBiome,
    WiledBiome,
}

[CreateAssetMenu(fileName = "NewFish", menuName = "Fishing/Fish", order = 0)]
public class FishScriptiableObject : ScriptableObject
{
    [Header("Fish Data")]
    [SerializeField] public string fishName;
    [SerializeField] private Sprite smallFishSprite;
    [SerializeField] private Sprite largeFishSprite;
    ///[SerializeField] private Sprite icon;
    [SerializeField] private BiomeType biomeType;
    public bool collected = false;
    [TextArea]
    [SerializeField] private string fishDescription;

    [SerializeField] private string fishWeight;
    [SerializeField] private string fishLength;

   
    public string FishName => fishName;
    public Sprite SmallFishSprite => smallFishSprite;
    public Sprite LargeFishSprite => largeFishSprite;
    public BiomeType BiomeType => biomeType;
    public string FishDescription => fishDescription;
    public string FishWeight => fishWeight;
    public string FishLength => fishLength;
    
    public BiomeType Biome => biomeType;
}
