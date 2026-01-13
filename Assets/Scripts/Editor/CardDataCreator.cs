using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardDataCreator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Cards/Create All Default Cards")]
    public static void CreateAllCards()
    {
        CreateBiomes();
        CreateCharacters();
        CreateSpells();
        AssetDatabase.SaveAssets();
        Debug.Log("Tüm kartlar oluşturuldu!");
    }

    static void CreateBiomes()
    {
        string path = "Assets/ScriptableObjects/Biomes/";
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Biomes");

        CreateBiome("Orman", BiomeType.Forest, new Color(0.2f, 0.6f, 0.2f), 2, 1, path);
        CreateBiome("Çöl", BiomeType.Desert, new Color(0.9f, 0.8f, 0.4f), 1, 0, path);
        CreateBiome("Dağ", BiomeType.Mountain, new Color(0.5f, 0.5f, 0.5f), 1, 2, path);
        CreateBiome("Bataklık", BiomeType.Swamp, new Color(0.3f, 0.4f, 0.3f), 2, 0, path);
    }

    static void CreateBiome(string name, BiomeType type, Color color, int atkBonus, int defBonus, string path)
    {
        BiomeCard biome = ScriptableObject.CreateInstance<BiomeCard>();
        biome.cardName = name;
        biome.biomeType = type;
        biome.biomeColor = color;
        biome.attackBonus = atkBonus;
        biome.defenseBonus = defBonus;
        biome.description = $"{name} bölgesi. +{atkBonus} Saldırı, +{defBonus} Savunma";
        
        AssetDatabase.CreateAsset(biome, path + name + ".asset");
    }

    static void CreateCharacters()
    {
        string path = "Assets/ScriptableObjects/Characters/";
        if (!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Characters");

        CreateCharacter("Okçu", CharacterClass.Archer, 8, 4, 1, BiomeType.Forest, 
            "Uzak mesafeden saldırır. Orman'da bonus alır.", path);
        CreateCharacter("Barbar", CharacterClass.Barbarian, 12, 5, 2, BiomeType.Mountain, 
            "Yüksek hasar ve dayanıklılık. Dağ'da bonus alır.", path);
        CreateCharacter("Suikastçi", CharacterClass.Assassin, 6, 6, 0, BiomeType.Swamp, 
            "Kritik vuruş ustası. Bataklık'ta bonus alır.", path);
    }

    static void CreateCharacter(string name, CharacterClass charClass, int hp, int atk, int def, 
        BiomeType preferred, string desc, string path)
    {
        CharacterCard character = ScriptableObject.CreateInstance<CharacterCard>();
        character.cardName = name;
        character.characterClass = charClass;
        character.maxHealth = hp;
        character.attack = atk;
        character.defense = def;
        character.preferredBiome = preferred;
        character.description = desc;
        
        AssetDatabase.CreateAsset(character, path + name + ".asset");
    }

    static void CreateSpells()
    {
        string path = "Assets/ScriptableObjects/Spells/";
        if (!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Spells");

        CreateSpell("Ateş Topu", SpellType.Damage, 5, true, 
            "Hedef düşmana 5 hasar verir.", path);
        CreateSpell("İyileştirme", SpellType.Heal, 4, false, 
            "Hedef karakteri 4 can iyileştirir.", path);
    }

    static void CreateSpell(string name, SpellType type, int power, bool targetEnemy, string desc, string path)
    {
        SpellCard spell = ScriptableObject.CreateInstance<SpellCard>();
        spell.cardName = name;
        spell.spellType = type;
        spell.power = power;
        spell.targetEnemy = targetEnemy;
        spell.description = desc;
        
        AssetDatabase.CreateAsset(spell, path + name + ".asset");
    }
#endif
}

