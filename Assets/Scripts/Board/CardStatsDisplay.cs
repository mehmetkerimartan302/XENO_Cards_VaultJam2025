using UnityEngine;
using TMPro;
using System.Collections;

public class CardStatsDisplay : MonoBehaviour
{
    private BoardCell linkedCell;
    private TextMeshPro statsText;
    private TextMeshProUGUI statsTextUI;
    private TextMeshProUGUI atkTextUI;
    private TextMeshProUGUI hpTextUI;
    private CharacterCard characterCard;
    private int lastHealth = -1;
    private bool isConsolidated = false;

    public void Initialize(BoardCell cell, TextMeshPro text, CharacterCard card)
    {
        linkedCell = cell;
        statsText = text;
        characterCard = card;
        UpdateStats();
    }
    
    public void InitializeUI(BoardCell cell, TextMeshProUGUI text, CharacterCard card)
    {
        linkedCell = cell;
        statsTextUI = text;
        characterCard = card;
        
        var allTexts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach(var t in allTexts)
        {
            if (t.gameObject.name == "AtkText") atkTextUI = t;
            if (t.gameObject.name == "HpText") hpTextUI = t;
        }
        
        UpdateStats();
    }

    void Update()
    {
        if (linkedCell == null || characterCard == null) return;
        if (statsText == null && statsTextUI == null) return;
        
        if (linkedCell.currentHealth != lastHealth)
        {
            UpdateStats();
        }
    }

    public IEnumerator ConsolidateStats()
    {
        isConsolidated = true;
        
        if (statsTextUI != null)
        {
            statsTextUI.fontSize = statsTextUI.fontSize * 1.3f;
            yield return new WaitForSeconds(0.4f);
            statsTextUI.fontSize = statsTextUI.fontSize / 1.3f;
        }
        
        UpdateStats();
        yield return new WaitForSeconds(0.3f);
    }

    void UpdateStats()
    {
        if (linkedCell == null || characterCard == null) return;
        
        lastHealth = linkedCell.currentHealth;
        
        int baseAttack = characterCard.attack + linkedCell.spellBonusDamage;
        int finalAttack = baseAttack;
        string attackBonus = "";
        string hpBonus = "";
        
        if (linkedCell.HasBiome())
        {
            BiomeType biomeType = linkedCell.placedBiome.biomeType;
            
            if (biomeType == BiomeType.Desert)
            {
                finalAttack -= 2;
                if (!isConsolidated) attackBonus += "<color=#FF3333>(-2)</color>";
            }
            
            int charAtkBonus = characterCard.GetBonusAttack(biomeType);
            if (charAtkBonus > 0)
            {
                finalAttack += charAtkBonus;
                if (!isConsolidated) attackBonus += $"<color=#55FF55>(+{charAtkBonus})</color>";
            }
            
            int charHpBonus = characterCard.GetBonusHealth(biomeType);
            if (charHpBonus > 0)
            {
                if (!isConsolidated) hpBonus = $"<color=#55FF55>(+{charHpBonus})</color>";
            }
        }
        
        if (atkTextUI != null)
        {
            atkTextUI.text = $"ATK:{finalAttack}";
            atkTextUI.color = Color.red;
            atkTextUI.faceColor = Color.red;
        }
        
        if (hpTextUI != null)
        {
            hpTextUI.text = $"HP:{lastHealth}";
            if (lastHealth <= 1) {
                hpTextUI.color = Color.red;
                hpTextUI.faceColor = Color.red;
            }
            else if (lastHealth <= characterCard.maxHealth / 2) {
                hpTextUI.color = new Color(1f, 0.65f, 0f);
                hpTextUI.faceColor = new Color(1f, 0.65f, 0f);
            }
            else {
                hpTextUI.color = Color.green;
                hpTextUI.faceColor = Color.green;
            }
        }

        if (statsTextUI != null && atkTextUI == null)
        {
            string hpColor = lastHealth <= characterCard.maxHealth / 2 ? "orange" : "#33FF33";
            if (lastHealth <= 1) hpColor = "#FF3333";
            string hpT = $"<b><size=32><color={hpColor}>HP:{lastHealth}</color></size></b>";
            string atkT = $"<b><size=32><color=#FF3333>ATK:{finalAttack}</color></size></b>";
            statsTextUI.text = $"{atkT}  {hpT}";
        }
        
        if (statsText != null)
            statsText.text = $"{finalAttack} / {lastHealth}";
    }
}

