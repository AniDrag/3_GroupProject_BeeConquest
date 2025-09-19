using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Not used directly. Other components derive from this class, but it inherits MonoBehaviour so it can be assigned to a GameObject.
/// </summary>
public class Stats : MonoBehaviour,IDamageable
{
    #region All Details
    // Character info 
    private string characterName = "name";
    private string characterRace = "race";
    private string characterClass = "class";

    // Level details
    private int characterLevel = 1;
    protected int maxXP;
    private float xpToLevelMulti = 1.5f;

    // Multipliers
    private float healthMulti = 1.2f;
    private float staminaMulti = 1f;
    private float physicalDefenseMulti = 1f;
    private float magicDefenseMulti = 1f;
    private float statusDefenseMulti = 1f;

    // Base Stats
    private int vitality = 1;
    private int strength = 1;
    private int dexterity = 1;
    private int agility = 1;
    private int luck = 1;

    // Derived Stats
    private int maxHealth;
    private int healthRegenPerSec;

    private int maxStamina;
    private int staminaRegenPerSec;

    private int physicalDefense;
    private int magicDefense;
    private int statusDefense;
    private Dictionary<string,Buff> buffs = new Dictionary<string,Buff>();
    private List<Buff> activeBuffs = new List<Buff>();

    #endregion

    #region Getters (Properties)
    public int CharacterLevel => characterLevel;
    public long LevelUpPrice => maxXP;

    public string CharacterName => characterName;
    public string CharacterRace => characterRace;
    public string CharacterClass => characterClass;

    public int MaxHealth => maxHealth;
    public int HealthRegenPerSec => healthRegenPerSec;

    public int MaxStamina => maxStamina;
    public int StaminaRegenPerSec => staminaRegenPerSec;

    public int PhysicalDefense => physicalDefense;
    public int MagicDefense => magicDefense;
    public int StatusDefense => statusDefense;

    public int Vitality => vitality;
    public int Strength => strength;
    public int Dexterity => dexterity;
    public int Agility => agility;
    public int Luck => luck;



    #endregion
    #region Setters
    public void SetMultipliers(
        float xpToLevelMulti = 1, float healthMulti = 1, float staminaMulti = 1,
        float physicalDefMulti = 1, float magicDefMulti = 1, float statusDefMulti = 1)
    {
        this.xpToLevelMulti = xpToLevelMulti;
        this.healthMulti = healthMulti;
        this.staminaMulti = staminaMulti;
        this.physicalDefenseMulti = physicalDefMulti;
        this.magicDefenseMulti = magicDefMulti;
        this.statusDefenseMulti = statusDefMulti;

        UpdateStats();
    }

    public void SetBaseStats(int vit = 1, int str = 1, int dex = 1, int agi = 1, int luc = 1)
    {
        vitality = vit;
        strength = str;
        dexterity = dex;
        agility = agi;
        luck = luc;
        UpdateStats();
    }
    public void SetLevel(int level)
    {
        characterLevel = level;
        UpdateStats();
    }
    public void SetName(string name) => characterName = name;
    public void SetRace(string race) => characterRace = race;
    public void SetClass(string characterClass) => this.characterClass = characterClass;
    #endregion
    public event System.Action OnBuffed;
    // overwriten by other users
    public virtual void LevelUp()
    {
        characterLevel++;
        UpdateStats();
    }
    public virtual void OnDeath()
    {
        Debug.Log("I died");
    }
    public virtual void TakeDamage(DamageData data)
    {
        Debug.Log("I took Damage");
    }
    public virtual void UpdateStats()
    {
        
        int lvl = Mathf.Max(1, characterLevel); // Avoid zero-level problems

        maxHealth = (int)(50 * vitality * healthMulti) * lvl;
        healthRegenPerSec = (int)(2 * healthMulti) * lvl;

        maxStamina = (int)(10 * agility * staminaMulti) * lvl;
        staminaRegenPerSec = (int)(1 * staminaMulti) * lvl;

        physicalDefense = (int)(3 * vitality * physicalDefenseMulti) * lvl;
        magicDefense = (int)(3 * vitality * magicDefenseMulti) * lvl;
        statusDefense = (int)(3 * vitality * statusDefenseMulti) * lvl;

        //maxXP = (int)(100 * xpToLevelMulti);
    }

    
    // Buff managment
    public void AddBuff(Buff buff)
    {
        if (buffs.ContainsKey(buff.Id))
        {
            Buff manageBuff = buffs[buff.Id];
            manageBuff.Duration = buff.Duration;
            manageBuff.Multiplier *= buff.Multiplier;
            manageBuff.FlatModifier += buff.FlatModifier;
            manageBuff.TimeRemaining = manageBuff.Duration;
        }
        else buffs.Add(buff.Id, buff);
        //// Example rule: if same buff already exists, refresh duration
        //var existing = activeBuffs.Find(b => b.Id == buff.Id);
        //if (existing != null)
        //{
        //    existing.TimeRemaining = buff.Duration;
        //    return;
        //}
        //
        //activeBuffs.Add(buff);
        //Debug.Log(buffs.Count +"all buffs active");
        UpdateStats();
    }

    public void UpdateBuffs(float deltaTime)
    {
        var expired = new List<string>();

        foreach (var key in buffs.Keys)
        {
            buffs[key].TimeRemaining -= deltaTime;
            if (buffs[key].IsExpired)
                expired.Add(key);
        }

        foreach (var key in expired)
            buffs.Remove(key);
        UpdateStats(); // <-- Recalc after removing buffs
    }

    public float GetModifiedStat(StatType stat, float baseValue)
    {
        float flat = 0f;
        float mult = 1f;

        foreach (var buff in buffs.Values)
        {
            if (buff.TargetStat == stat)
            {
                flat += buff.FlatModifier;
                mult *= buff.Multiplier;
            }
        }
        float newValue = (baseValue + flat) * mult;
        //Debug.Log(newValue + stat.ToString());
        return newValue;
    }
}

