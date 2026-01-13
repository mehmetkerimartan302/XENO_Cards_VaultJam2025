using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card Data")]
    public CardData cardData;
    
    [Header("UI Elements")]
    public Image cardImage;
    public Image artImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI statsText;
    
    [Header("Visual Settings")]
    public float hoverScale = 1.1f;
    public float hoverY = 20f;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Transform originalParent;
    private bool isDragging;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        originalScale = transform.localScale;
    }

    public void SetupCard(CardData data)
    {
        cardData = data;
        
        if (nameText != null)
            nameText.text = data.cardName;
        
        if (descText != null)
            descText.text = data.description;
        
        if (artImage != null && data.cardArt != null)
            artImage.sprite = data.cardArt;
        
        if (cardImage != null)
        {
            switch (data.cardType)
            {
                case CardType.Biome:
                    cardImage.color = new Color(0.4f, 0.7f, 0.4f);
                    break;
                case CardType.Character:
                    cardImage.color = new Color(0.4f, 0.4f, 0.8f);
                    break;
                case CardType.Spell:
                    cardImage.color = new Color(0.8f, 0.4f, 0.8f);
                    break;
            }
        }
        
        if (statsText != null)
        {
            if (data is CharacterCard charCard)
            {
                statsText.text = $"HP:{charCard.maxHealth} ATK:{charCard.attack} DEF:{charCard.defense}";
            }
            else if (data is SpellCard spellCard)
            {
                statsText.text = $"Power: {spellCard.power}";
            }
            else if (data is BiomeCard biomeCard)
            {
                statsText.text = $"+{biomeCard.attackBonus} ATK";
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            transform.localScale = originalScale * hoverScale;
            transform.localPosition += new Vector3(0, hoverY, 0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            transform.localScale = originalScale;
            transform.localPosition = originalPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;
        
        isDragging = true;
        originalPosition = transform.localPosition;
        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BoardCell cell = hit.collider.GetComponent<BoardCell>();
            if (cell != null && TryPlaceCard(cell))
            {
                Destroy(gameObject);
                return;
            }
        }
        
        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;
    }

    bool CanDrag()
    {
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
                DeckManager.Instance.RemoveBiomeFromHand(biome);
                GameManager.Instance.OnBiomePlaced();
                return true;
            }
        }
        else if (cardData is CharacterCard character)
        {
            if (cell.HasBiome() && !cell.HasCharacter())
            {
                cell.PlaceCharacter(character);
                DeckManager.Instance.RemoveCharacterFromHand(character);
                GameManager.Instance.OnCharacterPlaced();
                return true;
            }
        }
        else if (cardData is SpellCard spell)
        {
            CombatManager.Instance.ApplySpell(spell, cell);
            DeckManager.Instance.RemoveSpellFromHand(spell);
            GameManager.Instance.OnSpellCast();
            return true;
        }
        
        return false;
    }

    public void SetOriginalPosition(Vector3 pos)
    {
        originalPosition = pos;
    }
}

