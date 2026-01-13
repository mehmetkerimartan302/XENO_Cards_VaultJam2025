using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class CardZoomManager : MonoBehaviour
{
    public static CardZoomManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject zoomPanel;
    public Image cardImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    [Header("Animation Settings")]
    public float animationDuration = 0.25f;

    private int openFrame;
    private Coroutine currentAnimation;
    private CanvasGroup canvasGroup;
    private RectTransform panelRect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (zoomPanel != null)
        {
            canvasGroup = zoomPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = zoomPanel.AddComponent<CanvasGroup>();

            panelRect = zoomPanel.GetComponent<RectTransform>();
            
            SetupLayout();
        }

        HideCardInstant();
    }
    
    private void SetupLayout()
    {
        if (zoomPanel == null) return;
        
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 850);
            panelRect.anchoredPosition = Vector2.zero;
        }
        
        Image bgImage = zoomPanel.GetComponent<Image>();
        if (bgImage == null)
        {
            bgImage = zoomPanel.AddComponent<Image>();
        }
        bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);
        
        if (nameText != null)
        {
            RectTransform rt = nameText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(-40, 60);
            nameText.alignment = TMPro.TextAlignmentOptions.Center;
            nameText.fontSize = 42;
        }
        
        if (cardImage != null)
        {
            RectTransform rt = cardImage.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 50);
            rt.sizeDelta = new Vector2(480, 550);
        }
        
        if (descriptionText != null)
        {
            RectTransform rt = descriptionText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 70);
            rt.sizeDelta = new Vector2(-40, 70);
            descriptionText.alignment = TMPro.TextAlignmentOptions.Center;
            descriptionText.fontSize = 24;
        }
        
        if (statsText != null)
        {
            RectTransform rt = statsText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 20);
            rt.sizeDelta = new Vector2(-40, 55);
            statsText.alignment = TMPro.TextAlignmentOptions.Center;
            statsText.fontSize = 32;
        }
    }

    private void Update()
    {
        if (zoomPanel != null && zoomPanel.activeSelf)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.frameCount > openFrame)
            {
                HideCard();
            }
        }
    }

    public void ShowCard(CardData data)
    {
        if (data == null) return;

        openFrame = Time.frameCount;

        if (nameText != null)
        {
            if (data is CharacterCard charCard)
            {
                string className = charCard.characterClass switch
                {
                    CharacterClass.Barbarian => "TANK",
                    CharacterClass.Archer => "ARCHER",
                    CharacterClass.Assassin => "ASSASSIN",
                    _ => charCard.characterClass.ToString().ToUpper()
                };
                nameText.text = className;
            }
            else if (data is BiomeCard biomeCard)
            {
                string biomeName = biomeCard.biomeType switch
                {
                    BiomeType.Mountain => "ORE BIOME",
                    BiomeType.Forest => "GREEN BIOME",
                    BiomeType.Swamp => "DULL BIOME",
                    BiomeType.Desert => "VISCID BIOME",
                    _ => biomeCard.biomeType.ToString().ToUpper() + " BIOME"
                };
                nameText.text = biomeName;
            }
            else if (data is SpellCard)
            {
                nameText.text = "SPELL";
            }
            else
            {
                nameText.text = data.cardType.ToString().ToUpper();
            }
        }

        if (descriptionText != null)
        {
            if (data is CharacterCard charCard)
            {
                string biomeName = charCard.preferredBiome switch
                {
                    BiomeType.Mountain => "Ore Biome",
                    BiomeType.Forest => "Green Biome",
                    BiomeType.Swamp => "Dull Biome",
                    BiomeType.Desert => "Viscid Biome",
                    _ => charCard.preferredBiome.ToString() + " Biome"
                };
                
                string desc = charCard.characterClass switch
                {
                    CharacterClass.Archer => "Ranged attacker. Gets +3 ATK bonus in " + biomeName + ".",
                    CharacterClass.Barbarian => "Heavy tank. Gets +3 HP bonus in " + biomeName + ".",
                    CharacterClass.Assassin => "Critical hit master. 50% chance for double damage in " + biomeName + ".",
                    _ => "Character unit."
                };
                descriptionText.text = desc;
            }
            else if (data is BiomeCard biomeCard)
            {
                string desc = biomeCard.biomeType switch
                {
                    BiomeType.Forest => "Lush green terrain. Archers gain attack bonus here.",
                    BiomeType.Mountain => "Rocky highlands. Barbarians gain health bonus here.",
                    BiomeType.Swamp => "Dull, murky wetlands. Assassins can critical hit here.",
                    BiomeType.Desert => "Viscid, harsh desert. All units get -2 ATK penalty.",
                    _ => "Terrain type."
                };
                descriptionText.text = desc;
            }
            else if (data is SpellCard)
            {
                descriptionText.text = "Cast to deal damage or buff allies.";
            }
            else
            {
            }
        }

        if (cardImage != null)
        {
            if (data.cardArt != null)
            {
                cardImage.gameObject.SetActive(true);
                cardImage.sprite = data.cardArt;
                cardImage.preserveAspect = true;
            }
            else
            {
                cardImage.gameObject.SetActive(false);
            }
        }

        if (statsText != null)
        {
            if (data is CharacterCard charCard)
            {
                statsText.text = $"HP: {charCard.maxHealth}  |  ATK: {charCard.attack}";
            }
            else if (data is BiomeCard)
            {
                statsText.text = "";
            }
            else if (data is SpellCard)
            {
                statsText.text = "";
            }
            else
            {
                statsText.text = "";
            }
        }

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimatePanel(true));
    }

    public void HideCard()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimatePanel(false));
    }

    private void HideCardInstant()
    {
        if (zoomPanel != null)
        {
            zoomPanel.SetActive(false);
            if (panelRect != null) panelRect.localScale = Vector3.zero;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }
    }

    private IEnumerator AnimatePanel(bool open)
    {
        if (zoomPanel == null) yield break;

        if (open)
        {
            zoomPanel.SetActive(true);
            if (panelRect != null) panelRect.localScale = Vector3.zero;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        float elapsed = 0f;
        Vector3 startScale = open ? Vector3.zero : Vector3.one;
        Vector3 endScale = open ? Vector3.one : Vector3.zero;
        float startAlpha = open ? 0f : 1f;
        float endAlpha = open ? 1f : 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            float easeT = open ? (1 - Mathf.Pow(1 - t, 3)) : (t * t * t);

            if (panelRect != null)
                panelRect.localScale = Vector3.Lerp(startScale, endScale, easeT);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, easeT);

            yield return null;
        }

        if (panelRect != null) panelRect.localScale = endScale;
        if (canvasGroup != null) canvasGroup.alpha = endAlpha;

        if (!open)
            zoomPanel.SetActive(false);
    }
}


