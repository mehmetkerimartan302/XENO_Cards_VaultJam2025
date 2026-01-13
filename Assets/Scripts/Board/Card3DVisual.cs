using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Card3DVisual : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer artRenderer;
    public TextMeshPro nameText;
    public TextMeshPro statsText;
    public MeshRenderer cardMesh;
    
    [Header("Settings")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 2f;
    
    private Vector3 startPos;
    private CardData cardData;
    public bool isExiting = false;

    void Start()
    {
        startPos = transform.position;
        EnsureCollider();
        Debug.Log($"Card3DVisual STARTED on {gameObject.name}");
    }

    void Update()
    {
        if (isExiting) return;
        
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * 0.05f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("🖱️ Sol tık algılandı! (Card3DVisual)");

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"Raycast Hit: {hit.collider.gameObject.name}");

                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform) || transform.IsChildOf(hit.collider.transform))
                {
                    Debug.Log("✅ KART BULUNDU! Zoom açılıyor...");
                    if (cardData != null)
                    {
                        if (CardZoomManager.Instance != null)
                        {
                            CardZoomManager.Instance.ShowCard(cardData);
                        }
                        else
                        {
                            Debug.LogError("❌ CardZoomManager YOK!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ CardData NULL!");
                    }
                }
            }
        }
    }

    public void SetupVisual(CardData data)
    {
        cardData = data;
        
        if (nameText != null)
            nameText.text = data.cardName;
        
        if (artRenderer != null && data.cardArt != null)
            artRenderer.sprite = data.cardArt;
        
        if (cardMesh != null)
        {
            Color cardColor = Color.white;
            
            if (data is BiomeCard biome)
            {
                cardColor = biome.biomeColor;
            }
            else if (data is CharacterCard)
            {
                cardColor = new Color(0.3f, 0.3f, 0.8f);
            }
            else if (data is SpellCard)
            {
                cardColor = new Color(0.8f, 0.3f, 0.8f);
            }
            
            cardMesh.material.color = cardColor;
        }
        
        /*
        if (statsText != null)
        {
            if (data is CharacterCard charCard)
            {
                statsText.text = $"HP:{charCard.maxHealth}\nATK:{charCard.attack}";
            }
            else
            {
                statsText.text = "";
            }
        }
        */
    }


    void EnsureCollider()
    {
        if (GetComponent<Collider>() == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1f, 1.5f, 0.1f);
        }
    }

}

