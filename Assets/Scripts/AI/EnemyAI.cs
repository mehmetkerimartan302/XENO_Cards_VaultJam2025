using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;
    
    [Header("Enemy Cards")]
    public List<BiomeCard> enemyBiomes = new List<BiomeCard>();
    public List<CharacterCard> enemyCharacters = new List<CharacterCard>();
    public List<SpellCard> enemySpells = new List<SpellCard>();

    [Header("Timing")]
    public float placementDelay = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    public void PlaceEnemyBiomes()
    {
        StartCoroutine(PlaceBiomesSequence());
    }

    bool IsSmartTurn()
    {
        if (GameManager.Instance == null) return false;
        
        float probability = GameManager.Instance.currentStage == GameStage.Stage1 ? 0.5f : 0.75f;
        float roll = Random.value;
        bool isSmart = roll < probability;
        
        Debug.Log($"AI Turn Type: {(isSmart ? "SMART" : "RANDOM")} (Roll: {roll:F2}, Chance: {probability:F2})");
        return isSmart;
    }

    IEnumerator PlaceBiomesSequence()
    {
        var enemyCells = BoardManager.Instance.GetEnemyCells();
        List<BiomeCard> biomesToPlace = new List<BiomeCard>(enemyBiomes);
        BiomeCard[] placementOrder = new BiomeCard[3];

        if (IsSmartTurn())
        {
            
            List<CharacterCard> charactersInHand = new List<CharacterCard>(enemyCharacters);
            
            
            for (int i = 0; i < charactersInHand.Count && i < 3; i++)
            {
                BiomeType preferred = charactersInHand[i].preferredBiome;
                BiomeCard matchingBiome = biomesToPlace.Find(b => b.biomeType == preferred);
                
                if (matchingBiome != null)
                {
                    placementOrder[i] = matchingBiome;
                    biomesToPlace.Remove(matchingBiome);
                }
            }
        }
        
        
        for (int i = 0; i < 3; i++)
        {
            if (placementOrder[i] == null && biomesToPlace.Count > 0)
            {
                placementOrder[i] = biomesToPlace[0];
                biomesToPlace.RemoveAt(0);
            }
        }

        
        for (int i = 0; i < 3; i++)
        {
            if (placementOrder[i] != null)
            {
                enemyCells[i].PlaceBiome(placementOrder[i]);
                yield return new WaitForSeconds(placementDelay);
            }
        }
        
        GameManager.Instance?.OnEnemyBiomesPlaced();
    }

    public void PlaceEnemyCharacters()
    {
        StartCoroutine(PlaceCharactersSequence());
    }

    IEnumerator PlaceCharactersSequence()
    {
        var enemyCells = BoardManager.Instance.GetEnemyCells();
        List<CharacterCard> charsToPlace = new List<CharacterCard>(enemyCharacters);
        CharacterCard[] placementOrder = new CharacterCard[3];

        if (IsSmartTurn())
        {
            
            for (int i = 0; i < 3; i++)
            {
                if (enemyCells[i].HasBiome())
                {
                    CharacterCard match = charsToPlace.Find(c => c.preferredBiome == enemyCells[i].placedBiome.biomeType);
                    if (match != null)
                    {
                        placementOrder[i] = match;
                        charsToPlace.Remove(match);
                    }
                }
            }
        }
        
        
        for (int i = 0; i < 3; i++)
        {
            if (placementOrder[i] == null && charsToPlace.Count > 0)
            {
                int index = Random.Range(0, charsToPlace.Count);
                placementOrder[i] = charsToPlace[index];
                charsToPlace.RemoveAt(index);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (placementOrder[i] != null)
            {
                enemyCells[i].PlaceCharacter(placementOrder[i]);
                yield return new WaitForSeconds(placementDelay);
            }
        }
        
        GameManager.Instance?.OnEnemyCharactersPlaced();
    }

    public void SetupEnemySide() { }

    public void DoEnemyTurn() { }

    BoardCell FindWeakestPlayerCharacter()
    {
        var playerCells = BoardManager.Instance.GetPlayerCells();
        BoardCell weakest = null;
        int lowestHealth = int.MaxValue;
        
        foreach (var cell in playerCells)
        {
            if (cell.HasCharacter() && cell.currentHealth < lowestHealth)
            {
                lowestHealth = cell.currentHealth;
                weakest = cell;
            }
        }
        
        return weakest;
    }
    
    public void CastEnemySpells()
    {
        if (enemySpells.Count == 0) return;
        StartCoroutine(CastSpellsSequence());
    }
    
    IEnumerator CastSpellsSequence()
    {
        foreach (var spell in enemySpells)
        {
            BoardCell targetCell = null;
            
            if (spell.spellType == SpellType.Damage || spell.spellType == SpellType.SoulSiphon)
                targetCell = FindWeakestPlayerCharacter();
            else if (spell.spellType == SpellType.Heal || spell.spellType == SpellType.Buff)
                targetCell = FindWeakestEnemyCharacter();
            
            if (targetCell != null && targetCell.HasCharacter())
            {
                yield return StartCoroutine(ApplySpellEffect(spell, targetCell));
                yield return new WaitForSeconds(placementDelay);
            }
        }
    }
    
    BoardCell FindWeakestEnemyCharacter()
    {
        var enemyCells = BoardManager.Instance.GetEnemyCells();
        BoardCell weakest = null;
        int lowestHealth = int.MaxValue;
        
        foreach (var cell in enemyCells)
        {
            if (cell.HasCharacter() && cell.currentHealth < lowestHealth)
            {
                lowestHealth = cell.currentHealth;
                weakest = cell;
            }
        }
        
        return weakest;
    }
    
    IEnumerator ApplySpellEffect(SpellCard spell, BoardCell target)
    {
        GameObject spellObj = new GameObject("Enemy_Spell_" + spell.cardName);
        spellObj.transform.position = target.transform.position + Vector3.up * 3f; 
        spellObj.transform.localScale = Vector3.one * 1.5f;
        
        if (spell.cardArt != null)
        {
            SpriteRenderer sr = spellObj.AddComponent<SpriteRenderer>();
            sr.sprite = spell.cardArt;
            spellObj.transform.rotation = Quaternion.Euler(90, 0, 0);
            
            float targetSize = 0.7f; 
            float maxDimension = Mathf.Max(spell.cardArt.bounds.size.x, spell.cardArt.bounds.size.y);
            float scale = targetSize / maxDimension;
            
            spellObj.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(spellObj.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(1, 0.1f, 1.5f);
            cube.GetComponent<Renderer>().material.color = Color.magenta;
            spellObj.transform.localScale = Vector3.one;
        }

        float dropDuration = 0.5f;
        Vector3 startPos = spellObj.transform.position;
        Vector3 endPos = target.transform.position + Vector3.up * 0.8f; 
        
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            spellObj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / dropDuration);
            yield return null;
        }
        
        yield return new WaitForSeconds(0.8f);
        
        Destroy(spellObj);
        
        switch (spell.spellType)
        {
            case SpellType.Damage:
                target.TakeDamage(spell.power);
                break;
                
            case SpellType.Heal:
                target.currentHealth += spell.power;
                break;
                
            case SpellType.Buff:
                target.spellBonusDamage += spell.power;
                break;
                
            case SpellType.SoulSiphon:
                target.TakeDamage(spell.power);
                var healTarget = FindWeakestEnemyCharacter();
                if (healTarget != null)
                    healTarget.currentHealth += spell.power;
                break;
        }
    }
}

