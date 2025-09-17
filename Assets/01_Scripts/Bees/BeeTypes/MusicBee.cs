using UnityEngine;

public class MusicBee : BeeAI
{
    [SerializeField] private string beeName = "name";
    [SerializeField] private float buffEffectRadious;
    protected override void Start()
    {
        base.Start(); // <---- this is crucial!

        SetName(beeName);
    }
    public override void TriggerAbilityLogic(BeeAI bee, PlayerCore player, Vector3 origin)
    {
        // Boost collection strenght flat stat
        //base.TriggerAbilityLogic();
        //bee.collectionStrength *= 2;
        foreach (var playerBee in player.playerBees)
        {
            if (Vector3.Distance(bee.transform.position, playerBee.transform.position) > buffEffectRadious) return;
            Debug.Log("Added buff--> LOVe u XD");
            float increseChance = 25; //1. 10% increase to overall chance
            float actualChance = 1 + increseChance / 100;
            int flatStatincrease = 500;
            float buffDuration = 30;
            playerBee.AddBuff(new Buff("Love u XD", StatType.CollectionStrength, flatStatincrease, actualChance, buffDuration));
        }

    }
}
