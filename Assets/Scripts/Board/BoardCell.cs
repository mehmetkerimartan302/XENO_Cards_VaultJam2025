using UnityEngine;

public class BoardCell : MonoBehaviour
{
    public int row;
    public int column;
    public bool isPlayerSide;
    
    public BiomeCard placedBiome;
    public CharacterCard placedCharacter;
    public int currentHealth;
    public int spellBonusDamage = 0; 
    
    private Renderer cellRenderer;
    private Color originalColor;
    private bool isHighlighted;
    private GameObject currentBiomeObject; 

    void Awake()
    {
        cellRenderer = GetComponentInChildren<Renderer>();
        if (cellRenderer != null)
            originalColor = cellRenderer.material.color;
    }

    public bool HasBiome() => placedBiome != null;
    public bool HasCharacter() => placedCharacter != null;

    public void PlaceBiome(BiomeCard biome)
    {
        placedBiome = biome;
        
        
        if (currentBiomeObject != null) Destroy(currentBiomeObject);
        
        
        if (biome.modelPrefab != null)
        {
            
            
            
            Transform parent = transform;
            
            
            currentBiomeObject = Instantiate(biome.modelPrefab, parent);
            currentBiomeObject.transform.localPosition = biome.modelOffset;
            
            
            Vector3 globalRot = (BoardManager.Instance != null) ? BoardManager.Instance.globalBiomeRotation : Vector3.zero;
            currentBiomeObject.transform.localRotation = Quaternion.Euler(biome.modelRotation + globalRot);
            
            
            
            Vector3 targetScale = (biome.modelScale.magnitude < 0.01f) ? Vector3.one : biome.modelScale;
            
            
            StartCoroutine(AnimateBiomeSpawn(currentBiomeObject.transform, targetScale));

            
            if (GameManager.Instance != null)
                GameManager.Instance.PlaySFX(GameManager.Instance.biomePlaceSfx);

            
            if (cellRenderer != null)
            {
                cellRenderer.enabled = false; 
            }
            else
            {
                Debug.LogWarning("⚠️ BoardCell.PlaceBiome: cellRenderer NULL!");
            }
        }
        else
        {
            Debug.LogError($"❌ {biome.cardName} Data'sında 'Model Prefab' YOK! (Boş). Fallback kart mı kullanılıyor?");
            
            
            if (cellRenderer != null)
            {
                cellRenderer.enabled = true;
                cellRenderer.material.color = biome.biomeColor;
            }
        }
    }

    public void PlaceCharacter(CharacterCard character)
    {
        placedCharacter = character;
        currentHealth = character.maxHealth;
        
        
        if (HasBiome())
        {
            int hpBonus = character.GetBonusHealth(placedBiome.biomeType);
            currentHealth += hpBonus;
        }
        
        
        if (CellVisualManager.Instance != null)
            CellVisualManager.Instance.SpawnCardVisual(this, character, 0.25f);

        
        if (GameManager.Instance != null)
            GameManager.Instance.PlaySFX(GameManager.Instance.cardPlaceSfx);
    }

    public void TakeDamage(int damage)
    {
        if (!HasCharacter()) return;
        
        
        int totalDefense = placedCharacter.defense;
        
        int actualDamage = Mathf.Max(0, damage - totalDefense);
        currentHealth -= actualDamage;
        
        
        if (CellVisualManager.Instance != null && actualDamage > 0)
        {
             CellVisualManager.Instance.SpawnDamagePopup(transform.position, actualDamage);
        }
        
        if (currentHealth <= 0)
        {
            placedCharacter = null;
            currentHealth = 0;
        }
    }

    public int GetAttackPower()
    {
        return GetAttackPower(out bool _);
    }

    public int GetAttackPower(out bool isCritical)
    {
        isCritical = false;
        if (!HasCharacter()) return 0;
        
        int attack = placedCharacter.attack;
        
        if (HasBiome())
        {
            
            if (placedBiome.biomeType == BiomeType.Desert)
            {
                attack -= 2;
                Debug.Log($"🏜️ Çöl debuff'ı! {placedCharacter.cardName} -2 güç");
            }
            
            
            attack += placedCharacter.GetBonusAttack(placedBiome.biomeType);
            
            
            if (placedCharacter.CanCritical(placedBiome.biomeType))
            {
                isCritical = true;
                attack *= 2;
                Debug.Log($"💥 KRİTİK VURUŞ! {placedCharacter.cardName}");
            }
        }
        
        
        attack += spellBonusDamage;
        
        
        return Mathf.Max(0, attack);
    }

    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        if (highlight)
        {
            
            if (cellRenderer != null)
            {
                cellRenderer.enabled = true;
                cellRenderer.material.color = Color.yellow;
            }
        }
        else
        {
            
            if (currentBiomeObject != null)
            {
                
                if (cellRenderer != null) cellRenderer.enabled = false;
            }
            else
            {
                
                if (cellRenderer != null)
                {
                    cellRenderer.enabled = true;
                    cellRenderer.material.color = placedBiome != null ? placedBiome.biomeColor : originalColor;
                }
            }
        }
    }

    public void Clear()
    {
        placedBiome = null;
        placedCharacter = null;
        currentHealth = 0;
        spellBonusDamage = 0;
        
        if (currentBiomeObject != null) Destroy(currentBiomeObject);
        currentBiomeObject = null;
        
        if (cellRenderer != null)
        {
            cellRenderer.enabled = true;
            cellRenderer.material.color = originalColor;
        }
    }
    
    
    public BiomeCard RemoveBiome()
    {
        if (placedBiome == null) return null;
        if (placedCharacter != null) return null; 
        
        BiomeCard removed = placedBiome;
        placedBiome = null;
        
        if (currentBiomeObject != null) Destroy(currentBiomeObject);
        currentBiomeObject = null;
        
        if (cellRenderer != null)
        {
            cellRenderer.enabled = true;
            cellRenderer.material.color = originalColor;
        }
        
        return removed;
    }
    
    
    public CharacterCard RemoveCharacter()
    {
        if (placedCharacter == null) return null;
        
        CharacterCard removed = placedCharacter;
        placedCharacter = null;
        currentHealth = 0;
        spellBonusDamage = 0;
        
        
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Card_") || child.name.StartsWith("StatsCanvas_"))
            {
                Destroy(child.gameObject);
            }
        }
        
        return removed;
    }
    
    System.Collections.IEnumerator AnimateBiomeSpawn(Transform target, Vector3 finalScale)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        target.localScale = Vector3.zero; 
        
        while (elapsed < duration)
        {
            if (target == null) yield break; 
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            
            
            float ease = 1 - Mathf.Pow(1 - t, 3);
            
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, finalScale, ease);
            yield return null;
        }
        
        if (target != null) target.localScale = finalScale; 
    }
}
