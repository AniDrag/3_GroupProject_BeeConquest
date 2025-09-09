using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class FieldBuff
{
    public enum BuffType
    {
        PollinMultiplier,
        Regeneration,
    }

    public BuffType type;
    public float value;
    public float duration;     // in seconds
    public float startTime;    // when applied

    public bool IsExpired(float currentTime)
    {
        return currentTime - startTime >= duration;
    }
}

// Adding colors to the cells
public enum CellColor { Red, Blue, Green, Black, White }

public class FieldCell : MonoBehaviour
{
    private int ID = 0;
    // We will need color as well
    private CellColor cellColor;
    private float durability;
    private float durabilityRegen;
    [SerializeField, Range(1, 100)] private float pollinMultiplier;

    public float GetDurability => durability;
    public float GetPolinMultiplyer => pollinMultiplier;
    public int GetID => ID;
    public float DecreseDurability(int amount) => durability - amount;
    // Adding the opposite option as well.
    public float IncreaseDurability(int amount) => durability + amount;
    private List<FieldBuff> activeBuffs = new(); // Uh?

    public void BuffFieldCell(FieldBuff buff)
    {
        buff.startTime = Time.time;
        activeBuffs.Add(buff);

        ApplyBuff(buff);
    }

    private void ApplyBuff(FieldBuff buff)
    {
        switch (buff.type)
        {
            case FieldBuff.BuffType.PollinMultiplier:
                pollinMultiplier += buff.value;
                break;
            case FieldBuff.BuffType.Regeneration:
                durabilityRegen += buff.value;
                break;
        }
    }

    private void RemoveBuff(FieldBuff buff)
    {
        switch (buff.type)
        {
            case FieldBuff.BuffType.PollinMultiplier:
                pollinMultiplier -= buff.value;
                break;
            case FieldBuff.BuffType.Regeneration:
                durabilityRegen -= buff.value;
                break;
        }
    }


    private void FixedUpdate()
    {
        float currentTime = Time.time;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].IsExpired(currentTime))
            {
                RemoveBuff(activeBuffs[i]);

                // 🔔 Notify GameManager
                Game_Manager.instance?.OnBuffExpired(this, activeBuffs[i]);

                activeBuffs.RemoveAt(i);
            }
        }
    }
}
