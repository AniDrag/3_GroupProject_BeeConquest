using UnityEngine;

public class LightningBee : BeeAI
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
        //foreach (var cell in player.currentField)
        //{
        // 
        //
        //}

    }
}
