using UnityEngine;

public class DamageData 
{
    public int baseDamage;
    public DamageType type;
    public float critMultiplier = 1f;
    public bool IsCritical => critMultiplier > 1f;

    public DamageData(int baseDamage, DamageType type, float critMultiplier = 1f)
    {
        this.baseDamage = baseDamage;
        this.type = type;
        this.critMultiplier = critMultiplier;
    }

}
/// <summary>
/// True damage ignores defense
/// </summary>
public enum DamageType
{
    Physical,
    Magical,
    Status,
    True // Ignores defense
}
public interface IDamageable
{
    void TakeDamage(DamageData data);
}
