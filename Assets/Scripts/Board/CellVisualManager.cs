using UnityEngine;
using TMPro;

public class CellVisualManager : MonoBehaviour
{
    public static CellVisualManager Instance;
    
    [Header("Prefabs")]
    public GameObject card3DPrefab;
    public GameObject damagePopupPrefab; 
    
    void Awake()
    {
        Instance = this;
    }

    
    public void SpawnDamagePopup(Vector3 position, int amount)
    {
        if (damagePopupPrefab == null) return;
        
        
        Vector3 spawnPos = position + Vector3.up * 1.5f;
        
        GameObject popupObj = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
        
        
        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(amount);
        }
        
        
        popupObj.transform.LookAt(Camera.main.transform);
        popupObj.transform.Rotate(0, 180, 0); 
    }

    public void SpawnCardVisual(BoardCell cell, CardData data, float height = 0.1f)
    {
        GameObject cardObj;
        
        if (card3DPrefab == null)
        {
            cardObj = new GameObject("Card_" + data.cardName);
            cardObj.transform.position = cell.transform.position + Vector3.up * height;
            cardObj.transform.SetParent(cell.transform);
            
            if (data.cardArt != null)
            {
                SpriteRenderer sr = cardObj.AddComponent<SpriteRenderer>();
                sr.sprite = data.cardArt;
                sr.sortingOrder = (int)(height * 10);
                
                float scale = 2.0f / Mathf.Max(data.cardArt.bounds.size.x, data.cardArt.bounds.size.y);
                
                if (cell.isPlayerSide)
                {
                    cardObj.transform.localScale = Vector3.one * 0.073f;
                    cardObj.transform.localPosition = new Vector3(0, 0.88f, 0);
                    cardObj.transform.rotation = Quaternion.Euler(45, 0, 0);
                }
                else
                {
                    cardObj.transform.localScale = Vector3.one * 0.073f;
                    cardObj.transform.localPosition = new Vector3(0, 0.88f, 0);
                    cardObj.transform.rotation = Quaternion.Euler(45, 0, 0);
                }
                
                FloatingObject floater = cardObj.AddComponent<FloatingObject>();
                floater.floatHeight = 0.08f;
                floater.floatSpeed = 2f;
                
                BoxCollider col = cardObj.AddComponent<BoxCollider>();
                col.size = new Vector3(data.cardArt.bounds.size.x, data.cardArt.bounds.size.y, 0.1f);
                
                Card3DVisual visual = cardObj.AddComponent<Card3DVisual>();
                visual.SetupVisual(data);
            }
            else
            {
                GameObject cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubeObj.transform.SetParent(cardObj.transform);
                cubeObj.transform.localPosition = Vector3.zero;
                cubeObj.transform.localScale = new Vector3(0.8f, 0.02f, 1.2f);
                
                Renderer rend = cubeObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    if (data is BiomeCard biome)
                        rend.material.color = biome.biomeColor;
                    else if (data is CharacterCard)
                        rend.material.color = new Color(0.4f, 0.4f, 0.9f);
                }
                
                if (!cell.isPlayerSide)
                {
                    cardObj.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
            }
            
            
            if (data is CharacterCard charCard)
            {
                GameObject canvasObj = new GameObject("StatsCanvas_" + data.cardName);
                canvasObj.transform.SetParent(cell.transform);

                float yPos = cell.isPlayerSide ? 1.55f : 1.55f;
                float zPos = cell.isPlayerSide ? 1.2f : 0.3f;
                canvasObj.transform.localPosition = new Vector3(0, yPos, zPos);
                canvasObj.transform.rotation = Quaternion.Euler(45, 0, 0);
                canvasObj.transform.localScale = Vector3.one * 0.005f;
                
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = cell.isPlayerSide ? 100 : 50;
                RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(400, 80);
                
                GameObject atkObj = new GameObject("AtkText");
                atkObj.transform.SetParent(canvasObj.transform);
                atkObj.transform.localPosition = new Vector3(-60, 0, 0);
                atkObj.transform.localRotation = Quaternion.identity;
                atkObj.transform.localScale = Vector3.one;
                
                TMPro.TextMeshProUGUI atkTmp = atkObj.AddComponent<TMPro.TextMeshProUGUI>();
                atkTmp.alignment = TMPro.TextAlignmentOptions.Center;
                atkTmp.fontSize = 36;
                atkTmp.fontStyle = TMPro.FontStyles.Bold;
                atkTmp.color = Color.red;
                atkTmp.text = "ATK:" + charCard.attack;
                
                RectTransform atkRect = atkObj.GetComponent<RectTransform>();
                atkRect.sizeDelta = new Vector2(150, 60);
                
                GameObject hpObj = new GameObject("HpText");
                hpObj.transform.SetParent(canvasObj.transform);
                hpObj.transform.localPosition = new Vector3(60, 0, 0);
                hpObj.transform.localRotation = Quaternion.identity;
                hpObj.transform.localScale = Vector3.one;
                
                TMPro.TextMeshProUGUI hpTmp = hpObj.AddComponent<TMPro.TextMeshProUGUI>();
                hpTmp.alignment = TMPro.TextAlignmentOptions.Center;
                hpTmp.fontSize = 36;
                hpTmp.fontStyle = TMPro.FontStyles.Bold;
                hpTmp.color = Color.green;
                hpTmp.text = "HP:" + charCard.maxHealth;
                
                RectTransform hpRect = hpObj.GetComponent<RectTransform>();
                hpRect.sizeDelta = new Vector2(150, 60);
                
                CardStatsDisplay display = canvasObj.AddComponent<CardStatsDisplay>();
                display.InitializeUI(cell, atkTmp, charCard);
                
                FloatingObject textFloater = canvasObj.AddComponent<FloatingObject>();
                textFloater.floatHeight = 0.05f;
                textFloater.floatSpeed = 2f;
            }
        }
        else
        {
            cardObj = Instantiate(card3DPrefab, cell.transform);
            cardObj.transform.position = cell.transform.position + Vector3.up * height;
            
            if (!cell.isPlayerSide)
            {
                cardObj.transform.rotation = Quaternion.Euler(0, 0, 180); 
            }
            
            var visual = cardObj.GetComponent<Card3DVisual>();
            if (visual != null)
                visual.SetupVisual(data);
        }
    }

    
    public void RevealCard(BoardCell cell)
    {
        Transform visual = null;
        Transform stats = null;

        
        foreach(Transform t in cell.transform)
        {
            if (t.name.StartsWith("Card_")) visual = t;
            if (t.name.StartsWith("StatsCanvas_")) stats = t;
        }

        if (visual != null)
        {
            StartCoroutine(FlipCardAnimation(visual, stats));
        }
    }

    System.Collections.IEnumerator FlipCardAnimation(Transform card, Transform stats)
    {
        float duration = 0.5f;
        Quaternion startRot = card.rotation;
        Quaternion endRot = Quaternion.Euler(90, 0, 0); 

        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            
            card.rotation = Quaternion.Lerp(startRot, endRot, t);
            
            
            if (t >= 0.5f && stats != null && !stats.gameObject.activeSelf)
            {
                stats.gameObject.SetActive(true);
            }
            yield return null;
        }
        
        card.rotation = endRot;
        if (stats != null) stats.gameObject.SetActive(true);
    }
}
