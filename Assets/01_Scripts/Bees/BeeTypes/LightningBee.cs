using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBee : BasicBee
{
    [SerializeField] private string beeName = "name";
    [SerializeField] private float buffEffectRadious;
    private HashSet<BasicBee> visitedBees = new HashSet<BasicBee>();

    BasicBee lastBee;
    protected override void Start()
    {
        base.Start(); // <---- this is crucial!

        SetName(beeName);
    }
    public override void TriggerAbilityLogic(BasicBee bee, PlayerCore player, Vector3 origin)
    {
        // Start the coroutine so you can yield inside it
        // If this class is not a MonoBehaviour, call StartCoroutine on a MonoBehaviour instance (e.g. Game_Manager.instance)
        StartCoroutine(TriggerAbilityCoroutine(bee, player, origin));
    }

    private IEnumerator TriggerAbilityCoroutine(BasicBee bee, PlayerCore player, Vector3 origin)
    {
        visitedBees.Clear();
        visitedBees.Add(bee);

        long debugDamage = 0;
        int requestedLoops = bee.CharacterLevel + 8;

        int maxPossibleJumps = Mathf.Min(requestedLoops, Mathf.Max(0, player.allBees.Count - visitedBees.Count));
        lastBee = bee;
        Vector3 currentStart = lastBee.transform.position;

        for (int i = 0; i < maxPossibleJumps; i++)
        {
            var candidates = new List<BasicBee>(player.allBees.Count);
            for (int idx = 0; idx < player.allBees.Count; idx++)
            {
                var b = player.allBees[idx];
                if (b != null && !visitedBees.Contains(b)) candidates.Add(b);
            }

            if (candidates.Count == 0) break;

            BasicBee targetBee = candidates[Random.Range(0, candidates.Count)];
            candidates.Remove(targetBee);
            visitedBees.Add(targetBee);

            Vector3 targetPos = targetBee.transform.position;
            currentStart = lastBee.transform.position;
            if (player.currentField == null) break;
            var c = player.currentField.GetCellAtWorldPos(targetPos);
            if (c == null)
            {
                lastBee = targetBee;
                Debug.Log($"No cell found, skipping.{player.currentField.GetCellAtWorldPos(targetPos)}, where targetPos is {targetPos}");
                continue;
            }

            // VFX
            GameObject rail = AbilityVfxPooler.Instance.Get("LightningRail");
            if (rail != null)
            {
                var line = rail.GetComponent<LightningLineController>();
                if (line != null)
                {
                    rail.transform.position = Vector3.zero;
                    line.SetPositions(currentStart, targetPos);
                    rail.SetActive(true);
                }
            }

            GameObject movement = AbilityVfxPooler.Instance.Get("LightningMovement");    // key defined in pool config
            if (movement != null)
            {
                movement.transform.position = targetPos;
                movement.transform.LookAt(currentStart);
                movement.SetActive(true);
            }

            yield return new WaitForEndOfFrame();

            GameObject explosion = AbilityVfxPooler.Instance.Get("LightningExplosion");
            if (explosion != null)
            {
                explosion.transform.position = targetPos;
                explosion.SetActive(true);

                // Auto return when finished
                explosion.GetComponent<PoolableVfx>().StartAutoReturn(2f);

            }

            //VFX END

            Vector2Int cellCoordinates = player.currentField.GetCellArrayPosition(c);
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (j == 0 && k == 0) continue;
                    var cell = player.currentField.GetCellByXY(j + cellCoordinates.x, k + cellCoordinates.y);
                    if (cell == null) continue;

                    long damage = (long)(bee.modedFlowerDurabilityDamage * 4f);
                    if (cell.Color == bee.beeAtribute) damage *= 2L;
                    Game_Manager.instance.DecreaseCellDurability(bee, cell, damage);
                    debugDamage += damage;
                }
            }


            yield return new WaitForSeconds(.05f);

            AbilityVfxPooler.Instance.Return(rail);

            yield return new WaitForSeconds(0.1f);

            AbilityVfxPooler.Instance.Return(movement);

            // Write down the new last pos
            lastBee = targetBee;
        }

        visitedBees.Clear();
    }
}

