using System.Collections;
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
        // Start the coroutine so you can yield inside it
        // If this class is not a MonoBehaviour, call StartCoroutine on a MonoBehaviour instance (e.g. Game_Manager.instance)
        StartCoroutine(TriggerAbilityCoroutine(bee, player, origin));
    }

    private IEnumerator TriggerAbilityCoroutine(BeeAI bee, PlayerCore player, Vector3 origin)
    {
        long DebugDamage = 0;
        int loops = bee.CharacterLevel + 2;

        if (player.playerBees == null || player.playerBees.Count == 0)
            yield break;

        for (int i = 0; i < loops; i++)
        {
            // Random.Range for ints: max is exclusive — use Count to include last index
            var targetBee = player.playerBees[Random.Range(0, player.playerBees.Count)];
            if (targetBee == null) continue;

            var targetPos = targetBee.transform.position;
            var c = player.currentField.GetCellAtWorldPos(targetPos);
            if (c == null) continue;

            Vector2Int cellCoordinates = player.currentField.GetCellArrayPosition(c);

            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (j == 0 && k == 0) continue;
                    var cell = player.currentField.GetCellByXY(j + cellCoordinates.x, k + cellCoordinates.y);
                    if (cell == null) continue;

                    long damage = (long)(bee.collectionStrength * 10f);
                    if (cell.Color == bee.beeAttribute)
                        damage *= (long)2f;
                    Game_Manager.instance.DecreaseCellDurability(bee, cell, damage);
                    DebugDamage += damage;
                }
            }

            Debug.Log($"I damaged this much: {DebugDamage}, by jumping to {loops} bees.");

            yield return new WaitForSeconds(1f);
        }
    }
}

