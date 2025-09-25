using System.Linq;
using UnityEngine;

public class LovingBee : BasicBee
{
    [SerializeField] private string beeName = "name";
    protected override void Start()
    {
        base.Start(); // <---- this is crucial!

        SetName(beeName);
    }
    public override void TriggerAbilityLogic(BasicBee bee, PlayerCore player, Vector3 origin)
    {
        var explosion = AbilityVfxPooler.Instance.Get("LovingExplosion");
        explosion.transform.position = origin;
        explosion.SetActive(true);
        explosion.GetComponent<PoolableVfx>().StartAutoReturn(1f);

        //base.TriggerAbilityLogic();
        //bee.collectionStrength *= 2;
        float increseChance = 1.1f; // 10% increase to overall chance
        float buffDuration = 20 + bee.Luck / 5;
        int flatStatincrease = 0;

        string auraKey = "LovingAura";
        var allVfx = bee.GetComponentsInChildren<PoolableVfx>(true);
        PoolableVfx existingAura = allVfx.FirstOrDefault(v => v.gameObject.name.EndsWith($"_{auraKey}"));

        if (existingAura != null)
        {

            existingAura.StartAutoReturn(buffDuration);
        }
        else
        {
            var aura = AbilityVfxPooler.Instance.Get(auraKey);
            aura.transform.SetParent(bee.transform);
            aura.transform.localPosition = Vector3.zero;
            aura.gameObject.SetActive(true);

            aura.GetComponent<PoolableVfx>().StartAutoReturn(buffDuration);
        }

        foreach (var playerBee in player.allBees)
        {
            Debug.Log("Added buff--> LOVe u XD");

            playerBee.AddBuff(new Buff("Love u XD", StatType.SpawnTokenChance, flatStatincrease, increseChance, buffDuration));
        }

    }
}
