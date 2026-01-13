using UnityEngine;

public enum SpellType
{
    Damage,
    Heal,
    Buff,
    SoulSiphon
}

[CreateAssetMenu(fileName = "NewSpell", menuName = "Cards/Spell Card")]
public class SpellCard : CardData
{
    public SpellType spellType;
    public int power = 5;
    public bool targetEnemy = true;

    private void OnEnable()
    {
        cardType = CardType.Spell;
    }
}

