using UnityEngine;

public enum BiomeType
{
    Forest,
    Desert,
    Mountain,
    Swamp
}

[CreateAssetMenu(fileName = "NewBiome", menuName = "Cards/Biome Card")]
public class BiomeCard : CardData
{
    public BiomeType biomeType;
    [Header("3D Model Settings")]
    public GameObject modelPrefab;
    public Vector3 modelScale = Vector3.one;
    public Vector3 modelRotation = Vector3.zero;
    public Vector3 modelOffset = Vector3.zero;
    
    public int attackBonus = 1;
    public int defenseBonus = 1;
    public Color biomeColor = Color.green;

    private void OnEnable()
    {
        cardType = CardType.Biome;
    }
}

