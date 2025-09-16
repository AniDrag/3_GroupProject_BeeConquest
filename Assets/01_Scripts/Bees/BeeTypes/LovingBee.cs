using UnityEngine;

public class LovingBee : BeeAI
{
    public override void TriggerAbilityLogic(BeeAI bee, PlayerCore player)
    {
        //base.TriggerAbilityLogic();
        bee.collectionStrength *= 2;

    }
}
