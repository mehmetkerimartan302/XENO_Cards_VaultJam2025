using UnityEngine;
using System.Collections.Generic;

public class HandUI : MonoBehaviour
{
    public static HandUI Instance;
    
    [Header("Settings")]
    public GameObject cardPrefab;
    public Transform handContainer;
    public float cardSpacing = 120f;
    
    private List<CardUI> handCards = new List<CardUI>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPhaseChanged.AddListener(OnPhaseChanged);
        }
    }

    void OnPhaseChanged(GamePhase phase)
    {
        RefreshHand();
    }

    public void RefreshHand()
    {
        foreach (var card in handCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
        handCards.Clear();
        
        if (DeckManager.Instance == null || cardPrefab == null) return;
        
        List<CardData> allCards = new List<CardData>();
        
        GamePhase phase = GameManager.Instance.currentPhase;
        
        if (phase == GamePhase.PlacingBiomes)
        {
            foreach (var b in DeckManager.Instance.playerBiomes)
                allCards.Add(b);
        }
        else if (phase == GamePhase.PlacingCharacters)
        {
            foreach (var c in DeckManager.Instance.playerCharacters)
                allCards.Add(c);
        }
        else if (phase == GamePhase.CastingSpells)
        {
            foreach (var s in DeckManager.Instance.playerSpells)
                allCards.Add(s);
        }
        
        float totalWidth = (allCards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2;
        
        for (int i = 0; i < allCards.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            RectTransform rt = cardObj.GetComponent<RectTransform>();
            
            Vector3 pos = new Vector3(startX + i * cardSpacing, 0, 0);
            rt.anchoredPosition = pos;
            
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.SetupCard(allCards[i]);
                cardUI.SetOriginalPosition(pos);
            }
            
            handCards.Add(cardUI);
        }
    }
}

