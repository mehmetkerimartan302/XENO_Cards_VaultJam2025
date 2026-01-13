using UnityEngine;
using UnityEngine.InputSystem;

public class CardPickup : MonoBehaviour
{
    private BoardCell cell;
    
    void Awake()
    {
        cell = GetComponent<BoardCell>();
    }
    
    void Update()
    {
        if (Mouse.current == null) return;

        bool rightClicked = Mouse.current.rightButton.wasPressedThisFrame;
        bool shiftLeftClicked = Keyboard.current != null && 
                               Keyboard.current.shiftKey.isPressed && 
                               Mouse.current.leftButton.wasPressedThisFrame;

        if (rightClicked || shiftLeftClicked)
        {
            Debug.Log("🖱️ Geri alma girişi algılandı!");
            
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("❌ Camera.main bulunamadı! Lütfen kameranın tag'ini 'MainCamera' yapın.");
                return;
            }

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log($"🎯 Raycast şuna çarptı: {hit.collider.gameObject.name} (Hücre: {gameObject.name})");
                
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    TryPickupCard();
                }
            }
        }
    }
    
    void TryPickupCard()
    {
        Debug.Log($"🔍 {gameObject.name} geri alma deneniyor... Phase: {GameManager.Instance.currentPhase}");
        if (cell == null) return;
        if (!cell.isPlayerSide) 
        {
            Debug.Log("🚫 Bu hücre oyuncu tarafında değil!");
            return;
        }
        
        var phase = GameManager.Instance.currentPhase;
        
        if (phase == GamePhase.PlacingCharacters && cell.HasCharacter())
        {
            CharacterCard card = cell.RemoveCharacter();
            if (card != null)
            {
                DeckManager.Instance?.ReturnCardToHand(card);
                GameManager.Instance.OnCardPickedUp(CardType.Character);
                Debug.Log($"🔙 {card.cardName} geri alındı!");
            }
        }
        else if (phase == GamePhase.PlacingBiomes && cell.HasBiome() && !cell.HasCharacter())
        {
            BiomeCard card = cell.RemoveBiome();
            if (card != null)
            {
                DeckManager.Instance?.ReturnCardToHand(card);
                GameManager.Instance.OnCardPickedUp(CardType.Biome);
                Debug.Log($"🔙 {card.cardName} geri alındı!");
            }
        }
    }
}

