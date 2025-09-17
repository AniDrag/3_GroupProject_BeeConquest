using UnityEngine;

public class LovingBee : BeeAI
{
    [SerializeField] private string beeName = "name";
    protected override void Start()
    {
        base.Start(); // <---- this is crucial!

        SetName(beeName);
    }
    public override void TriggerAbilityLogic(BeeAI bee, PlayerCore player, Vector3 origin)
    {
        //base.TriggerAbilityLogic();
        //bee.collectionStrength *= 2;
        foreach(var playerBee in player.playerBees)
        {
            Debug.Log("Added buff--> LOVe u XD");
            float increseChance = 1.1f; // 10% increase to overall chance
            float buffDuration = 20;
            int flatStatincrease = 0;
            playerBee.AddBuff(new Buff("Love u XD", StatType.SpawnTokenChance, flatStatincrease, increseChance, buffDuration));
        }

    }
}
