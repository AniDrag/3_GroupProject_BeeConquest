using UnityEngine;

[System.Serializable]
public class Buff
{
    public string Id;                 // "Haste", "Poison", "BeeFrenzy"
    public StatType TargetStat;
    public float FlatModifier;        // +10
    public float Multiplier;     // x1.2 bas multiplayer is one so plus 10% would be .1f so 1+.1f = 110% aka 1.1f
    public float Duration;            // in seconds
    public float TimeRemaining;// tick down
    public int stack; // max stack

    public bool IsExpired => TimeRemaining <= 0f;

    public Buff(string id, StatType stat, float flat, float mult, float duration)
    {
        Id = id;
        TargetStat = stat;
        FlatModifier = flat;
        Multiplier = mult;
        Duration = duration;
        TimeRemaining = duration;
    }
}

public enum StatType
{
    Vitality,
    Strength,
    Dexterity,
    Agility,
    MaxHealth,
    MaxStamina,
    Speed,
    CollectionStrength,
    CollectionSpeed,
    CritDamage,
    PhysicalDefense,
    MagicDefense,
    StatusDefense,
    SpawnTokenChance
}

