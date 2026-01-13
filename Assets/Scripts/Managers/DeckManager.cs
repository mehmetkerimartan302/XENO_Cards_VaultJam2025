using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    
    [Header("All Cards")]
    public List<BiomeCard> allBiomes = new List<BiomeCard>();
    public List<CharacterCard> allCharacters = new List<CharacterCard>();
    public List<SpellCard> allSpells = new List<SpellCard>();
    
    [Header("Player Hand")]
    public List<BiomeCard> playerBiomes = new List<BiomeCard>();
    public List<CharacterCard> playerCharacters = new List<CharacterCard>();
    public List<SpellCard> playerSpells = new List<SpellCard>();

    void Awake()
    {
        Instance = this;
        CreateFallbackCardsIfEmpty();
    }
    
    void CreateFallbackCardsIfEmpty()
    {
        if (allBiomes.Count == 0)
        {
            var b1 = ScriptableObject.CreateInstance<BiomeCard>();
            b1.cardName = "Forest"; b1.biomeType = BiomeType.Forest; b1.biomeColor = new Color(0, 0.5f, 0);
            b1.attackBonus = 0; b1.defenseBonus = 0;
            allBiomes.Add(b1);
            
            var b2 = ScriptableObject.CreateInstance<BiomeCard>();
            b2.cardName = "Desert"; b2.biomeType = BiomeType.Desert; b2.biomeColor = new Color(0.8f, 0.8f, 0);
            b2.attackBonus = 0; b2.defenseBonus = -2;
            allBiomes.Add(b2);
            
            var b3 = ScriptableObject.CreateInstance<BiomeCard>();
            b3.cardName = "Mountain"; b3.biomeType = BiomeType.Mountain; b3.biomeColor = Color.gray;
            b3.attackBonus = 1; b3.defenseBonus = 1;
            allBiomes.Add(b3);
            
            var b4 = ScriptableObject.CreateInstance<BiomeCard>();
            b4.cardName = "Swamp"; b4.biomeType = BiomeType.Swamp; b4.biomeColor = new Color(0.3f, 0.2f, 0.3f);
            b4.attackBonus = -1; b4.defenseBonus = 0;
            allBiomes.Add(b4);
        }
        
        if (allCharacters.Count == 0)
        {
            var c1 = ScriptableObject.CreateInstance<CharacterCard>();
            c1.cardName = "Archer"; c1.characterClass = CharacterClass.Archer; c1.preferredBiome = BiomeType.Forest;
            c1.maxHealth = 7; c1.attack = 3; c1.defense = 0;
            allCharacters.Add(c1);
            
            var c2 = ScriptableObject.CreateInstance<CharacterCard>();
            c2.cardName = "Barbarian"; c2.characterClass = CharacterClass.Barbarian; c2.preferredBiome = BiomeType.Mountain;
            c2.maxHealth = 10; c2.attack = 3; c2.defense = 1;
            allCharacters.Add(c2);
            
            var c3 = ScriptableObject.CreateInstance<CharacterCard>();
            c3.cardName = "Assassin"; c3.characterClass = CharacterClass.Assassin; c3.preferredBiome = BiomeType.Swamp;
            c3.maxHealth = 5; c3.attack = 5; c3.defense = 0;
            allCharacters.Add(c3);
        }

        if (allSpells.Count == 0)
        {
             var s1 = ScriptableObject.CreateInstance<SpellCard>();
             s1.cardName = "Fireball"; s1.spellType = SpellType.Damage; s1.power = 2; 
             allSpells.Add(s1);
             
             var s2 = ScriptableObject.CreateInstance<SpellCard>();
             s2.cardName = "Heal"; s2.spellType = SpellType.Heal; s2.power = 3; 
             allSpells.Add(s2);
        }
    }

    public void DealCards()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlaySFX(GameManager.Instance.cardPlaceSfx);
            
        playerBiomes.Clear();
        playerCharacters.Clear();
        playerSpells.Clear();
        
        if (EnemyAI.Instance != null)
        {
            EnemyAI.Instance.enemyBiomes.Clear();
            EnemyAI.Instance.enemyCharacters.Clear();
            EnemyAI.Instance.enemySpells.Clear();
        }
        
        List<BiomeCard> tempBiomes = new List<BiomeCard>(allBiomes);
        for (int i = 0; i < 3 && tempBiomes.Count > 0; i++)
        {
            int index = Random.Range(0, tempBiomes.Count);
            playerBiomes.Add(tempBiomes[index]);
            tempBiomes.RemoveAt(index);
        }
        
        List<CharacterCard> tempChars = new List<CharacterCard>(allCharacters);
        
        bool isStage2 = GameManager.Instance != null && GameManager.Instance.currentStage == GameStage.Stage2;
        bool cavalryGuaranteed = isStage2 && cavalryCard != null && Random.value < 0.6f;
        
        if (cavalryGuaranteed && tempChars.Contains(cavalryCard))
        {
            playerCharacters.Add(cavalryCard);
            tempChars.Remove(cavalryCard);
        }
        
        int remaining = cavalryGuaranteed ? 2 : 3;
        for (int i = 0; i < remaining && tempChars.Count > 0; i++)
        {
            int index = Random.Range(0, tempChars.Count);
            playerCharacters.Add(tempChars[index]);
            tempChars.RemoveAt(index);
        }
        
        if (allSpells.Count > 0)
        {
            int index = Random.Range(0, allSpells.Count);
            playerSpells.Add(allSpells[index]);
        }

        if (EnemyAI.Instance != null)
        {
            
            List<BiomeCard> enemyTempBiomes = new List<BiomeCard>(allBiomes);
            List<BiomeType> pickedBiomeTypes = new List<BiomeType>();
            
            for (int i = 0; i < 3 && enemyTempBiomes.Count > 0; i++)
            {
                int index = Random.Range(0, enemyTempBiomes.Count);
                BiomeCard picked = enemyTempBiomes[index];
                EnemyAI.Instance.enemyBiomes.Add(picked);
                pickedBiomeTypes.Add(picked.biomeType);
                enemyTempBiomes.RemoveAt(index);
            }
            
            
            List<CharacterCard> enemyTempChars = new List<CharacterCard>(allCharacters);
            
            
            CharacterCard luckyMatch = enemyTempChars.Find(c => pickedBiomeTypes.Contains(c.preferredBiome));
            if (luckyMatch != null)
            {
                EnemyAI.Instance.enemyCharacters.Add(luckyMatch);
                enemyTempChars.Remove(luckyMatch);
            }
            
            
            int remainingToFill = luckyMatch != null ? 2 : 3;
            for (int i = 0; i < remainingToFill && enemyTempChars.Count > 0; i++)
            {
                int index = Random.Range(0, enemyTempChars.Count);
                EnemyAI.Instance.enemyCharacters.Add(enemyTempChars[index]);
                enemyTempChars.RemoveAt(index);
            }
            
            if (allSpells.Count > 0)
            {
                int index = Random.Range(0, allSpells.Count);
                EnemyAI.Instance.enemySpells.Add(allSpells[index]);
            }
        }
    }

    public void RemoveBiomeFromHand(BiomeCard biome)
    {
        playerBiomes.Remove(biome);
    }

    public void RemoveCharacterFromHand(CharacterCard character)
    {
        playerCharacters.Remove(character);
    }

    public void RemoveSpellFromHand(SpellCard spell)
    {
        playerSpells.Remove(spell);
    }
    
    public void ReturnCardToHand(CardData card)
    {
        if (card is BiomeCard biome)
            playerBiomes.Add(biome);
        else if (card is CharacterCard character)
            playerCharacters.Add(character);
        else if (card is SpellCard spell)
            playerSpells.Add(spell);
        
        SimpleHandUI handUI = FindAnyObjectByType<SimpleHandUI>();
        if (handUI != null)
            handUI.RefreshHand();
    }

    [Header("Special Cards")]
    public CharacterCard cavalryCard;
    private bool cavalryAdded = false;

    public void AddCavalryToDeck()
    {
        if (cavalryAdded || cavalryCard == null) return;
        
        allCharacters.Add(cavalryCard);
        cavalryAdded = true;
        Debug.Log("Süvari kartı desteye eklendi!");
    }

    public void ResetDeck()
    {
        if (cavalryAdded && cavalryCard != null)
        {
            allCharacters.Remove(cavalryCard);
            cavalryAdded = false;
        }
    }
}

