using UnityEngine;
using TMPro;

public class TextStyleUtility : MonoBehaviour
{
    [Header("Style Preset")]
    public TextStyle style = TextStyle.Stats;

    public enum TextStyle
    {
        Default,
        Title,
        Stats,
        Description
    }

    private TMP_Text textComponent;

    void Start()
    {
        ApplyStyle();
    }

    public void ApplyStyle()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent == null) return;

        switch (style)
        {
            case TextStyle.Title:
                textComponent.fontSize = 32;
                textComponent.fontStyle = FontStyles.Bold;
                break;

            case TextStyle.Stats:
                textComponent.fontSize = 22;
                textComponent.fontStyle = FontStyles.Bold;
                break;

            case TextStyle.Description:
                textComponent.fontSize = 16;
                textComponent.fontStyle = FontStyles.Normal;
                break;

            default:
                break;
        }
    }
}







