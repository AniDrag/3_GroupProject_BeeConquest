using System.Linq;
using UnityEngine;

public class MusicBee : BasicBee
{
    [SerializeField] private string beeName = "name";
    [SerializeField] private float buffEffectRadious;
    protected override void Start()
    {
        base.Start(); // <---- this is crucial!

        SetName(beeName);
    }
    public override void TriggerAbilityLogic(BasicBee bee, PlayerCore player, Vector3 origin)
    {
        // Boost collection strenght flat stat
        //base.TriggerAbilityLogic();
        //bee.collectionStrength *= 2;
        float increseChance = 25; //1. 10% increase to overall chance
        float actualChance = 1 + increseChance / 100;
        int flatStatincrease = 500;
        float buffDuration = 10;

        GameObject explosion = AbilityVfxPooler.Instance.Get("MeloBeeAbility");
        if (explosion != null)
        {
            explosion.transform.position = bee.transform.position;
            explosion.SetActive(true);
            explosion.GetComponent<PoolableVfx>().StartAutoReturn(2f);
        }
        foreach (var playerBee in player.allBees)
        {
            if (Vector3.Distance(bee.transform.position, playerBee.transform.position) > buffEffectRadious) return;
            Debug.Log("Added buff--> LOVe u XD");



            // Check if this type of buff already exists.
            string auraKey = "MeloBeeAura";
            var allVfx = playerBee.GetComponentsInChildren<PoolableVfx>(true);
            PoolableVfx existingAura = allVfx.FirstOrDefault(v => v.gameObject.name.EndsWith($"_{auraKey}"));

            if (existingAura != null)
            {

                existingAura.StartAutoReturn(buffDuration);
            }
            else
            {
                var aura = AbilityVfxPooler.Instance.Get(auraKey);
                aura.transform.SetParent(playerBee.transform);
                aura.transform.localPosition = Vector3.zero;
                aura.gameObject.SetActive(true);

                aura.GetComponent<PoolableVfx>().StartAutoReturn(buffDuration);
            }

            playerBee.AddBuff(new Buff("Love u XD", StatType.CollectionStrength, flatStatincrease, actualChance, buffDuration));
        }

    }
}
