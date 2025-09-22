using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBee : BasicBee
{
    [SerializeField] private string beeName = "name";
    [SerializeField] private float buffEffectRadious;

    [SerializeField] private GameObject lightningPrefab;
    private LightningLineController lightningLineController;
    private GameObject myLightningVFX;
    private HashSet<BasicBee> visitedBees = new HashSet<BasicBee>();

    BasicBee lastBee;
    protected override void Start()
    {
        base.Start(); // <---- this is crucial!

        SetName(beeName);
        GameObject newAbilityVFX = Instantiate(lightningPrefab);
        newAbilityVFX.SetActive(false);
        lightningLineController = newAbilityVFX.GetComponent<LightningLineController>();
        myLightningVFX = newAbilityVFX;
    }
    public override void TriggerAbilityLogic(BasicBee bee, PlayerCore player, Vector3 origin)
    {
        // Start the coroutine so you can yield inside it
        // If this class is not a MonoBehaviour, call StartCoroutine on a MonoBehaviour instance (e.g. Game_Manager.instance)
        StartCoroutine(TriggerAbilityCoroutine(bee, player, origin));
    }

    private IEnumerator TriggerAbilityCoroutine(BasicBee bee, PlayerCore player, Vector3 origin)
    {
        myLightningVFX.SetActive(true);
        visitedBees.Clear();
        visitedBees.Add(bee);

        long debugDamage = 0;
        int requestedLoops = bee.CharacterLevel + 1;

        if (player.allBees == null || player.allBees.Count == 0)
        {
            myLightningVFX.SetActive(false);
            yield break;
        }

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

            var c = player.currentField.GetCellAtWorldPos(targetPos);
            if (c == null)
            {
                lastBee = targetBee;
                continue;
            }

            if (lightningLineController != null)
                lightningLineController.SetPositions(currentStart, targetPos);


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

            Debug.Log($"I damaged this much: {debugDamage}, by jumping to {targetBee.name} bee. Current iteration {i} / {maxPossibleJumps}");
            yield return new WaitForSeconds(.1f);

            if (lightningLineController != null)
                lightningLineController.ClearLines();

            yield return new WaitForSeconds(0.45f);


            lastBee = targetBee;
        }

        myLightningVFX.SetActive(false);
        visitedBees.Clear();
    }
}

