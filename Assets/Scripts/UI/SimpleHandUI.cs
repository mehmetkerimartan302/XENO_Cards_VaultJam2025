using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleHandUI : MonoBehaviour
{
    public static SimpleHandUI Instance;
    
    [Header("Prefab")]
    public GameObject cardPrefab;
    
    [Header("Layout")]
    public Transform cardContainer;
    public float cardWidth = 165f;
    public float cardHeight = 240f;
    public float spacing = 25f;
    
    private List<DraggableCard> currentCards = new List<DraggableCard>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onPhaseChanged.AddListener(OnPhaseChanged);
        
        Invoke(nameof(DelayedRefresh), 0.5f);
    }

    void DelayedRefresh()
    {
        RefreshHand();
    }

    public void RefreshHand()
    {
        if (GameManager.Instance != null)
            OnPhaseChanged(GameManager.Instance.currentPhase);
    }

    void OnPhaseChanged(GamePhase phase)
    {
        if (DeckManager.Instance == null) return;
        
        List<CardData> cardsToShow = new List<CardData>();
        
        switch (phase)
        {
            case GamePhase.PlacingBiomes:
                foreach (var b in DeckManager.Instance.playerBiomes)
                    cardsToShow.Add(b);
                break;
                
            case GamePhase.PlacingCharacters:
                foreach (var c in DeckManager.Instance.playerCharacters)
                    cardsToShow.Add(c);
                break;
                
            case GamePhase.CastingSpells:
                foreach (var s in DeckManager.Instance.playerSpells)
                    cardsToShow.Add(s);
                break;
        }
        
        UpdateHandDisplay(cardsToShow);
    }

    void UpdateHandDisplay(List<CardData> newHandData)
    {
        if (cardContainer == null) return;

        for (int i = currentCards.Count - 1; i >= 0; i--)
        {
            if (currentCards[i] == null || !newHandData.Contains(currentCards[i].cardData))
            {
                if (currentCards[i] != null) Destroy(currentCards[i].gameObject);
                currentCards.RemoveAt(i);
            }
        }

        foreach (var data in newHandData)
        {
            bool exists = false;
            foreach (var existing in currentCards)
            {
                if (existing != null && existing.cardData == data)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                GameObject cardObj = cardPrefab != null ? Instantiate(cardPrefab, cardContainer) : CreateSimpleCard();
                cardObj.transform.SetParent(cardContainer);
                cardObj.transform.localScale = Vector3.one;
                
                DraggableCard draggable = cardObj.GetComponent<DraggableCard>();
                if (draggable == null) draggable = cardObj.AddComponent<DraggableCard>();
                
                draggable.Setup(data);
                currentCards.Add(draggable);
                
                RectTransform rt = cardObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cardWidth, cardHeight);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -250f);
            }
        }

        float totalWidth = currentCards.Count * cardWidth + (currentCards.Count - 1) * spacing;
        float startX = -totalWidth / 2 + cardWidth / 2;

        for (int i = 0; i < currentCards.Count; i++)
        {
            if (currentCards[i] == null) continue;
            
            RectTransform rt = currentCards[i].GetComponent<RectTransform>();
            Vector2 targetPos = new Vector2(startX + i * (cardWidth + spacing), 0);
            
            StopCoroutine("AnimateCardToPos");
            StartCoroutine(AnimateCardToPos(rt, targetPos));
        }
    }

    System.Collections.IEnumerator AnimateCardToPos(RectTransform rt, Vector2 targetPos)
    {
        if (rt == null) yield break;
        
        Vector2 startPos = rt.anchoredPosition;
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (rt == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            
            if (startPos.y < -100f)
                rt.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, easeT);
            
            yield return null;
        }
        
        if (rt != null)
        {
            rt.anchoredPosition = targetPos;
            rt.localScale = Vector3.one;
        }
    }

    GameObject CreateSimpleCard()
    {
        GameObject card = new GameObject("Card");
        
        Image img = card.AddComponent<Image>();
        img.color = Color.white;
        
        GameObject statsContainer = new GameObject("StatsContainer");
        statsContainer.transform.SetParent(card.transform, false);
        RectTransform containerRt = statsContainer.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 1f);
        containerRt.anchorMax = new Vector2(0.5f, 1f);
        containerRt.pivot = new Vector2(0.5f, 0f);
        containerRt.sizeDelta = new Vector2(250, 40);
        containerRt.anchoredPosition = new Vector2(0, 10);
        
        UnityEngine.UI.HorizontalLayoutGroup layout = statsContainer.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.spacing = 8;
        
        GameObject atkObj = new GameObject("AtkText");
        atkObj.transform.SetParent(statsContainer.transform, false);
        TMPro.TextMeshProUGUI atkText = atkObj.AddComponent<TMPro.TextMeshProUGUI>();
        atkText.font = TMPro.TMP_Settings.defaultFontAsset;
        atkText.fontSize = 24;
        atkText.fontStyle = TMPro.FontStyles.Bold;
        atkText.color = Color.red; 
        atkText.alignment = TMPro.TextAlignmentOptions.Center;
        
        GameObject hpObj = new GameObject("HpText");
        hpObj.transform.SetParent(statsContainer.transform, false);
        TMPro.TextMeshProUGUI hpText = hpObj.AddComponent<TMPro.TextMeshProUGUI>();
        hpText.font = TMPro.TMP_Settings.defaultFontAsset;
        hpText.fontSize = 24;
        hpText.fontStyle = TMPro.FontStyles.Bold;
        hpText.color = Color.green;
        hpText.alignment = TMPro.TextAlignmentOptions.Center;
        
        return card;
    }

    void ClearHand()
    {
        foreach (var card in currentCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
        currentCards.Clear();
    }

    public void RemoveCard(DraggableCard card)
    {
        currentCards.Remove(card);
    }
}

