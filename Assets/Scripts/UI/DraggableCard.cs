using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector3 startPosition;
    private Transform startParent;
    private BoardCell currentHighlightedCell;
    private GameObject bonusPreviewObj;
    
    private static readonly Color BIOME_COLOR = new Color(0.3f, 0.7f, 0.3f);
    private static readonly Color CHARACTER_COLOR = new Color(0.3f, 0.4f, 0.8f);
    private static readonly Color SPELL_COLOR = new Color(0.8f, 0.3f, 0.6f);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
        
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            if (data.cardArt != null)
            {
                image.sprite = data.cardArt;
                image.color = Color.white;
            }
            else
            {
                if (data is BiomeCard)
                    image.color = BIOME_COLOR;
                else if (data is CharacterCard)
                    image.color = CHARACTER_COLOR;
                else if (data is SpellCard)
                    image.color = SPELL_COLOR;
            }
        }
        
        var allTexts = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI atkText = null;
        TMPro.TextMeshProUGUI hpText = null;
        
        foreach (var t in allTexts)
        {
            if (t.gameObject.name == "AtkText") atkText = t;
            if (t.gameObject.name == "HpText") hpText = t;
        }
        
        if (data is CharacterCard charCard)
        {
            if (atkText != null) {
                atkText.text = $"ATK:{charCard.attack}";
                atkText.color = Color.red;
                atkText.faceColor = Color.red;
            }
            if (hpText != null) {
                hpText.text = $"HP:{charCard.maxHealth}";
                hpText.color = Color.green;
                hpText.faceColor = Color.green;
            }
        }
        else if (data is SpellCard spellCard)
        {
            if (atkText != null) {
                atkText.text = GetSpellDescription(spellCard);
                Color vibrantGreen = new Color(0.2f, 1f, 0.2f, 1f);
                atkText.color = vibrantGreen;
                atkText.faceColor = vibrantGreen;
                atkText.richText = true;
                atkText.outlineColor = Color.black;
                atkText.outlineWidth = 0.15f; 
                atkText.ForceMeshUpdate();
            }
            if (hpText != null) {
                hpText.text = "";
                hpText.color = Color.white;
            }
        }
        else
        {
            string txt = data.cardArt != null ? "" : data.cardName;
            if (atkText != null) {
                atkText.text = txt;
                atkText.color = Color.white;
            }
            if (hpText != null) hpText.text = "";
        }
    }
    
    string GetSpellDescription(SpellCard spell)
    {
        switch (spell.spellType)
        {
            case SpellType.Damage:
                return $"+{spell.power} ATK";
            case SpellType.Heal:
                return $"+{spell.power} HP";
            case SpellType.Buff:
                return $"+{spell.power} ATK";
            case SpellType.SoulSiphon:
                return $"STEAL {spell.power}";
            default:
                return spell.cardName;
        }
    }
    
    string GetSpellDescriptionLegacy(SpellCard spell)
    {
        switch (spell.spellType)
        {
            case SpellType.Damage:
                return $"+{spell.power} ATK";
            case SpellType.Heal:
                return $"+{spell.power} HP";
            case SpellType.Buff:
                return $"+{spell.power} ATK";
            case SpellType.SoulSiphon:
                return $"STEAL {spell.power}";
            default:
                return spell.cardName;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;
        
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;
        currentHighlightedCell = null;
        
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        UpdateHoverHighlight(eventData.position);
    }
    
    void UpdateHoverHighlight(Vector2 screenPosition)
    {
        if (Camera.main == null) return;
        
        if (currentHighlightedCell != null)
        {
            currentHighlightedCell.SetHighlight(false);
            currentHighlightedCell = null;
        }
        
        if (bonusPreviewObj != null)
        {
            Destroy(bonusPreviewObj);
            bonusPreviewObj = null;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        
        foreach (var hit in hits)
        {
            BoardCell cell = hit.collider.GetComponentInParent<BoardCell>();
            if (cell != null && CanPlaceOnCell(cell))
            {
                cell.SetHighlight(true);
                currentHighlightedCell = cell;
                ShowBonusPreview(cell);
                break;
            }
        }
    }
    
    void ShowBonusPreview(BoardCell cell)
    {
        CharacterCard charCard = cardData as CharacterCard;
        BiomeCard biomeCard = cardData as BiomeCard;
        SpellCard spellCard = cardData as SpellCard;
        
        string bonusText = "";
        bool shouldShow = false;

        if (charCard != null && cell.HasBiome()) 
        {
            bonusText = GetBonusText(charCard, cell.placedBiome.biomeType);
            shouldShow = !string.IsNullOrEmpty(bonusText);
        }
        else if (biomeCard != null && cell.HasCharacter())
        {
            charCard = cell.placedCharacter;
            bonusText = GetBonusText(charCard, biomeCard.biomeType);
            shouldShow = !string.IsNullOrEmpty(bonusText);
        }
        else if (spellCard != null && cell.HasCharacter())
        {
            bonusText = GetSpellDescriptionLegacy(spellCard);
            shouldShow = true;
        }

        if (!shouldShow || string.IsNullOrEmpty(bonusText)) return;
        
        bonusPreviewObj = new GameObject("BonusPreview");
        bonusPreviewObj.transform.SetParent(transform);
        
        RectTransform previewRect = bonusPreviewObj.AddComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.5f, 1f);
        previewRect.anchorMax = new Vector2(0.5f, 1f);
        previewRect.pivot = new Vector2(0.5f, 0f);
        previewRect.sizeDelta = new Vector2(120, 40);
        previewRect.anchoredPosition = new Vector2(0, 50);
        
        UnityEngine.UI.Image bg = bonusPreviewObj.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bonusPreviewObj.transform);
        
        TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = bonusText;
        tmp.fontSize = 32;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.richText = false;
        
        if (bonusText.Contains("-")) {
            tmp.color = new Color(1f, 0.1f, 0.1f, 1f);
        } else {
            tmp.color = new Color(0.1f, 1f, 0.1f, 1f);
        }
        tmp.faceColor = tmp.color;
        tmp.alpha = 1.0f;
        tmp.outlineColor = Color.black;
        tmp.outlineWidth = 0.15f;
        tmp.ForceMeshUpdate();
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
    }
    
    string GetBonusText(CharacterCard charCard, BiomeType biomeType)
    {
        string result = "";
        
        if (biomeType == BiomeType.Desert)
            result = "-2 ATK";
        
        if (biomeType == charCard.preferredBiome)
        {
            switch (charCard.characterClass)
            {
                case CharacterClass.Archer:
                    result += (result.Length > 0 ? "\n" : "") + "+3 ATK!";
                    break;
                case CharacterClass.Barbarian:
                    result += (result.Length > 0 ? "\n" : "") + "+3 HP!";
                    break;
                case CharacterClass.Assassin:
                    result += (result.Length > 0 ? "\n" : "") + "CRIT!";
                    break;
            }
        }
        
        return string.IsNullOrEmpty(result) ? null : result;
    }
    
    bool CanPlaceOnCell(BoardCell cell)
    {
        if (!cell.isPlayerSide) return false;
        
        if (cardData is BiomeCard)
            return !cell.HasBiome();
        else if (cardData is CharacterCard)
            return cell.HasBiome() && !cell.HasCharacter();
        else if (cardData is SpellCard)
            return cell.HasCharacter();
        
        return false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentHighlightedCell != null)
        {
            currentHighlightedCell.SetHighlight(false);
            currentHighlightedCell = null;
        }
        
        if (bonusPreviewObj != null)
        {
            Destroy(bonusPreviewObj);
            bonusPreviewObj = null;
        }
        
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        
        if (Camera.main == null)
        {
            ReturnToHand();
            return;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        
        foreach (var hit in hits)
        {
            BoardCell cell = hit.collider.GetComponentInParent<BoardCell>();
            if (cell != null)
            {
                if (TryPlaceCard(cell))
                {
                    SimpleHandUI.Instance?.RemoveCard(this);
                    Destroy(gameObject);
                    return;
                }
            }
        }
        
        ReturnToHand();
    }
    
    void ReturnToHand()
    {
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
    }

    bool CanDrag()
    {
        if (GameManager.Instance == null) return false;
        
        GamePhase phase = GameManager.Instance.currentPhase;
        
        if (cardData is BiomeCard && phase == GamePhase.PlacingBiomes)
            return true;
        if (cardData is CharacterCard && phase == GamePhase.PlacingCharacters)
            return true;
        if (cardData is SpellCard && phase == GamePhase.CastingSpells)
            return true;
        
        return false;
    }

    bool TryPlaceCard(BoardCell cell)
    {
        if (!cell.isPlayerSide) return false;
        
        if (cardData is BiomeCard biome)
        {
            if (!cell.HasBiome())
            {
                cell.PlaceBiome(biome);
                DeckManager.Instance?.RemoveBiomeFromHand(biome);
                GameManager.Instance?.OnBiomePlaced();
                return true;
            }
        }
        else if (cardData is CharacterCard character)
        {
            if (cell.HasBiome() && !cell.HasCharacter())
            {
                cell.PlaceCharacter(character);
                DeckManager.Instance?.RemoveCharacterFromHand(character);
                GameManager.Instance?.OnCharacterPlaced();
                return true;
            }
        }
        else if (cardData is SpellCard spell)
        {
            CombatManager.Instance?.ApplySpell(spell, cell);
            DeckManager.Instance?.RemoveSpellFromHand(spell);
            GameManager.Instance?.OnSpellCast();
            return true;
        }
        
        return false;
    }
}

