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
    private int characterLevel = 0;
    private int levelUpPrice;
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

    // Derived Stats
    private int maxHealth;
    private int healthRegenPerSec;

    private int maxStamina;
    private int staminaRegenPerSec;

    private int physicalDefense;
    private int magicDefense;
    private int statusDefense;

#endregion

    #region Getters (Properties)
    public int CharacterLevel => characterLevel;
    public long LevelUpPrice => levelUpPrice;

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

    public void SetBaseStats(int vit = 1, int str = 1, int dex = 1, int agi = 1)
    {
        vitality = vit;
        strength = str;
        dexterity = dex;
        agility = agi;
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

        levelUpPrice = (int)(100 * xpToLevelMulti);
    }
}
