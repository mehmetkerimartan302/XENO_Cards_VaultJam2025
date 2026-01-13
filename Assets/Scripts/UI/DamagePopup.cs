using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Color textColor;
    private float disappearTimer;
    private float moveYSpeed = 2f;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null) textMesh.text = "";
    }

    public void Setup(int damageAmount)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>();
        
        textMesh.text = damageAmount.ToString();
        textMesh.fontStyle = TMPro.FontStyles.Bold;
        
        textColor = Color.red;
        textMesh.color = textColor;
        textMesh.faceColor = textColor;
        
        disappearTimer = 1f;

        moveYSpeed = 2f;
        transform.localPosition += new Vector3(Random.Range(-0.2f, 0.2f), 0, 0);
    }

    void Update()
    {
        transform.position += new Vector3(0, moveYSpeed * Time.deltaTime, 0);

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

