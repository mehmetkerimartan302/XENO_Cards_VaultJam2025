using UnityEngine;

public enum CharacterClass
{
    Archer,
    Barbarian,
    Assassin
}

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Cards/Character Card")]
public class CharacterCard : CardData
{
    public CharacterClass characterClass;
    public int maxHealth = 10;
    public int attack = 3;
    public int defense = 2;
    public BiomeType preferredBiome;

    private void OnEnable()
    {
        cardType = CardType.Character;
    }

    public int GetBonusAttack(BiomeType currentBiome)
    {
        if (currentBiome != preferredBiome) return 0;
        
        if (characterClass == CharacterClass.Archer)
            return 3;
        
        return 0;
    }

    public int GetBonusHealth(BiomeType currentBiome)
    {
        if (currentBiome != preferredBiome) return 0;
        
        if (characterClass == CharacterClass.Barbarian)
            return 3;
        
        return 0;
    }

    public bool CanCritical(BiomeType currentBiome)
    {
        if (characterClass == CharacterClass.Assassin && currentBiome == preferredBiome)
            return Random.value < 0.5f;
        return false;
    }
}

