using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Managing leveling up, health and other small things like status effects, resistances and so on.
/// Major calculations will be stored here.
/// </summary>
public class PlayerCore : Stats
{
    private int currentXP;
    private int currentHealth;
    private int currentStamina;
    private int currentMana;

    #region Geters
    public int Stamina => currentStamina;
    public int Mana => currentMana;
    public int Health => currentHealth;
    #endregion

    #region Basic Functions (Awakem, Start, Update)
    private void Awake()
    {
        SetBaseStats();
        SetMultipliers();
    }
    private void Start()
    {
        UpdateStats();
        currentHealth = MaxHealth;
        currentStamina = MaxStamina;
        currentMana = MaxMagicules;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    public void AddXp(int XP)
    {
        currentXP += XP;
        if (currentXP >= XpToLevelUp)
        {
            currentXP -= XpToLevelUp;
            Debug.Log("player leveld up");
            LevelUp();

        }
        else
        {
            Debug.Log($"Aquired {XP}xp, current XP = {currentXP}");
        }
    }
    private int DamageCalculation(DamageType damageType, int baseDamage)
    {
        int rawDamage = baseDamage;
        // Add scaling (you can tweak these multipliers later)
        switch (damageType)
        {
            case DamageType.Physical:
                rawDamage -= PhysicalDefense;
                break;

            case DamageType.Magical:                
                rawDamage -= MagicDefense;
                break;

            case DamageType.Status:
                rawDamage -= StatusDefense;
                break;
            case DamageType.True:
                // Ignores defense
                break;
        }
        return rawDamage;
    }

    #region Overide functions
    public override void OnDeath()// whatever happens when the player dies
    {
        base.OnDeath();
    }
    public override void TakeDamage(DamageData data)
    {
        int rawDamage;

        // Apply crit if present
        rawDamage = Mathf.RoundToInt(DamageCalculation(data.type, data.baseDamage) * data.critMultiplier);

        // Clamp and apply
        int finalDamage = Mathf.Max(0, rawDamage);
        currentHealth -= finalDamage;

        Debug.Log($"Took {finalDamage} {data.type} damage{(data.IsCritical ? " (CRIT!)" : "")}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath();
        }
    }

    #endregion
}

