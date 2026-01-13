using UnityEngine;

public class CardHoverEffect : MonoBehaviour
{
    public float originalHeight = 0.5f;
    public float hoverScale = 1.2f;
    public float hoverSpeed = 10f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private SpriteRenderer spriteRenderer;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverSpeed);
        
        if (spriteRenderer != null)
        {
            Color targetColor = isHovered ? new Color(1f, 1f, 0.8f, 1f) : Color.white;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * hoverSpeed);
        }
    }

    void OnMouseEnter()
    {
        isHovered = true;
        targetScale = originalScale * hoverScale;
    }

    void OnMouseExit()
    {
        isHovered = false;
        targetScale = originalScale;
    }

    public void ResetScale()
    {
        isHovered = false;
        targetScale = originalScale;
        transform.localScale = originalScale;
    }
}

