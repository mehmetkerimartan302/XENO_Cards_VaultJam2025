using UnityEngine;

public enum CardType
{
    Biome,
    Character,
    Spell
}

public abstract class CardData : ScriptableObject
{
    public string cardName;
    public Sprite cardArt;
    [TextArea] public string description;
    public CardType cardType;
}

