using AniDrag.Utility;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerCore : MonoBehaviour
{
    // [SerializeField, ShowIf("showBeeData")] public BeeAI[] testBee;
    // [ShowIf("showBeeData")] private Dictionary<int, List<BeeAI>> beeGroups = new Dictionary<int, List<BeeAI>>();// this is a dictionary of 3 squads of warrior and defender bees the player can asign adn then manipulate

    [Header("----- UI refrences -----")]
    [SerializeField] public UI_Visuals visualsUI;
    [SerializeField] public UI_Dialogue dialogueUI;
    [SerializeField] public UI_Quests questUI;


    [Header("----- Bee Data -----")]
    public bool showBeeData;
    [SerializeField, Range(5, 25), ShowIf("showBeeData")] int startFollowDistance = 5;
    [SerializeField, ShowIf("showBeeData")] int spawnNumPerClick = 75;
    [SerializeField, ShowIf("showBeeData")] GameObject BeePRF;
    [SerializeField, ShowIf("showBeeData"), Range(20, 1000)] int beeBatchPerUpdate = 100;
    //[ShowIf("showBeeData")] public List<BeeAI> playerBees { get; private set; } = new List<BeeAI>();
    [ShowIf("showBeeData")] public List<BasicBee> allBees { get; private set; } = new List<BasicBee>();
    public List<PlayerBeeSaved> savedBees = new List<PlayerBeeSaved>();

    public bool isConvertingPollen;

    [Header("----- Field Data -----")]
    public FieldGenerator currentField { get; private set; }


    [Header("----- Inventory Data -----")]
    public int pollinStorageLevel { get; private set; } = 1;

    [SerializeField] private float pollinStorageMULTI = 1.3f;
    [SerializeField] private long currentPollinAmount = 0;
    [SerializeField] private long maxPollinStorage = 100;
    private long _baseMaxPollinStorage;
    public long GetCurrentPollin => currentPollinAmount;
    public Dictionary<BeeFood,int> foodStorage = new Dictionary<BeeFood,int>();
    [SerializeField] public int ownedCellsAmount { get; private set; } = 0;
    [SerializeField] public long currentHoneyAmount { get; private set; } = 0;
    [SerializeField] private bool showReceivedHoney = true;
    [SerializeField] private bool showReceivedPollen = true;
    private Queue<long> honeyQueue = new Queue<long>();

    private int batchTracker;


    #region Getters
    public int playerID { get; private set; } = 0;

    #endregion

    private void Awake()
    {
        PlayerServerData data = new PlayerServerData(playerID, transform, this, savedBees) { };
        Game_Manager.instance.JoinServer(playerID, data);
        currentHoneyAmount = 10000000;
        maxPollinStorage = 9_823_872;

        _baseMaxPollinStorage = Math.Max(1L, maxPollinStorage);
    }
    void Start()
    {
        visualsUI.pollinCounterText.text = $"Pollin: {currentPollinAmount}/{maxPollinStorage}";
        visualsUI.honeyCounterText.text = $"Nicterial: {currentHoneyAmount}";
        for (int i = 0; i < spawnNumPerClick; i++)
        {
            GameObject newBee = Instantiate(BeePRF);

            BasicBee beeTwo = newBee.GetComponent<BasicBee>();

            beeTwo.SetMyParent(this);
            allBees.Add(beeTwo);
            PlayerBeeSaved newSavedBee = new PlayerBeeSaved(beeTwo,newBee);
            savedBees.Add(newSavedBee);

            newBee.transform.position = transform.position + Vector3.up;

        }
    }

    private float playerStateUpdateInterval = .2f;
    private float playerRareTimer = 0f;
    private float playerNextRareTime = 0f;

    private float perSecond = 10;
    private float polinPerSecTime = 0f;
    private float pollinPerSecRareTime = 0f;
    private long oldPollinAmount = 0;
    private void FixedUpdate()
    {
        //if (playerBees.Count > 0) Debug.Log(playerBees.Count + " Bee amount from Player");
        if (allBees.Count > 0)
        {
            for (int i = 0; i < allBees.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, allBees[i].destinationPoint);
                if (distance >= startFollowDistance && currentField == null &&
                    allBees[i].StateMachine.currentState != allBees[i].pollinCollectionState &&
                    allBees[i].StateMachine.currentState != allBees[i].combatState &&
                    !isConvertingPollen)
                {
                    //Debug.Log("player requested bee to follow DISTANCE:" + distance + " current bool is - " + isConvertingPollen);
                    //Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(playerBees[i]);
                    allBees[i].SetDestination(this.transform.position);
                    allBees[i].StateMachine.ChangeState(allBees[i].chaseState);
                }

            }
        }



        playerRareTimer += Time.fixedDeltaTime;
        polinPerSecTime += Time.fixedDeltaTime;
        if (playerRareTimer >= playerNextRareTime)
        {
            playerRareTimer = 0f;
            playerNextRareTime = Mathf.Max(0.01f, playerStateUpdateInterval);
            long honeySum = 0;
            while (honeyQueue.TryDequeue(out var val))
            {
                honeySum += val;
            }
            ActuallyShowHoneyVisual(honeySum);
            if (allBees == null || allBees.Count == 0) return;
            //FollowPlayerLogic();
        }
        if (polinPerSecTime >= pollinPerSecRareTime)
        {
            //Debug.Log("Pollin per sec update");
            polinPerSecTime = 0f;
            pollinPerSecRareTime = Mathf.Max(0.01f, perSecond);
            float perSec = (currentPollinAmount - oldPollinAmount) / perSecond;
            if (perSec < 0) perSec = 0;
            visualsUI.pollinPerSecText.text = $"{perSec:F1}/s";
            oldPollinAmount = currentPollinAmount;
        }


    }
    #region Collection and currency FUNCTIONS
    public long RemovePollin(long amount)
    {
        long tempNum = amount;
        if (amount > currentPollinAmount)
        {
            tempNum = currentPollinAmount;
            currentPollinAmount = 0;
            visualsUI.pollinCounterText.text = $"Pollin: {currentPollinAmount}/{maxPollinStorage}";
            visualsUI.UI_UpdatePollin(currentPollinAmount, maxPollinStorage);
            return tempNum;
        }
        else
        {
            currentPollinAmount -= amount;
            visualsUI.pollinCounterText.text = $"Pollin: {currentPollinAmount}/{maxPollinStorage}";
            visualsUI.UI_UpdatePollin(currentPollinAmount, maxPollinStorage);
            return amount;
        }

    }
    public void AddPollin(long pollen, long honey)
    {
        currentPollinAmount += pollen;
        //Debug.Log("What da fak" + currentPollinAmount + " I GOT " + pollen);
        if(currentPollinAmount >= maxPollinStorage)
        {
            currentPollinAmount = maxPollinStorage;
            RemoveField();
        }
        visualsUI.pollinCounterText.text = $"Pollin: {currentPollinAmount}/{maxPollinStorage}";
        visualsUI.UI_UpdatePollin(currentPollinAmount, maxPollinStorage);
    }
    public void AddHoney(long amount)
    {
        currentHoneyAmount += amount;
        ShowHoneyVisual(amount);
        visualsUI.pollinCounterText.text = $"Pollin: {currentPollinAmount}/{maxPollinStorage}";
        visualsUI.honeyCounterText.text = $"Nicterial: {currentHoneyAmount}";
        visualsUI.UI_UpdatePollin(currentPollinAmount, maxPollinStorage);
    }

    public void RemoveHoney(long amount)
    {
        currentHoneyAmount -= amount;
        visualsUI.pollinCounterText.text = $"Pollin: {currentPollinAmount}/{maxPollinStorage}";
        visualsUI.honeyCounterText.text = $"Nicterial: {currentHoneyAmount}";
        visualsUI.UI_UpdatePollin(currentPollinAmount, maxPollinStorage);
    }
    public void ShowPollinVisual(long pollen, Vector3 position, ColorAtribute color)
    {
        if (showReceivedPollen == true)
            FloatingLabelPool.Instance.ShowAmount(pollen, position, color.ToColor());
    }

    public void ShowHoneyVisual(long honeyReceived)
    {
        if (showReceivedHoney && honeyReceived > 0)
            honeyQueue.Enqueue(honeyReceived);
    }

    private void ActuallyShowHoneyVisual(long honey)
    {
        if (showReceivedHoney && honey > 0)
            FloatingLabelPool.Instance.ShowAmount(honey, transform.position + Vector3.up, Color.yellow);
    }
    #endregion

    #region Field Associated Functions
    public void AsignField(FieldGenerator newField)
    {// by not having a field asigned we bees will not collect pollin
        if (currentPollinAmount >= maxPollinStorage) return;

        currentField = newField;
    }
    public void RemoveField()
    {
        currentField = null;
    }
    #endregion

    #region Bee and Bee Controll Functions
    [Button("SpawnBees", ButtonSize.Medium, 0, 0, 0, 1, SdfIconType.None)]
    public void SpawnBees()
    {
        for (int i = 0; i < spawnNumPerClick; i++)
        {
            GameObject newBee = Instantiate(BeePRF);

            BasicBee beeTwo = newBee.GetComponent<BasicBee>();
            beeTwo.SetMyParent(this);
            allBees.Add(beeTwo);
            PlayerBeeSaved newSavedBee = new PlayerBeeSaved(beeTwo, newBee);
            savedBees.Add(newSavedBee);
            //Game_Manager.instance.players[playerID].playerBeesTwo.Add(bee);

        }
    }
    //void FollowPlayerLogic()
    //{
    //    if (currentField == null)
    //    {
    //        float distance = Vector3.Distance(Game_Manager.instance.players[playerID].lastKnownPosition, this.transform.position);
    //        if (distance < 2f) return;

    //        int count = allBees.Count;
    //        int processed = 0;
    //        while (processed < beeBatchPerUpdate && count > 0)
    //        {
    //            int idx = (batchTracker + processed) % count;
    //            var bee = allBees[idx];

    //            float beeToPlayerDist = Vector3.Distance(transform.position, bee.transform.position);
    //            if (beeToPlayerDist > startFollowDistance && bee.beeState != BeeState.Collecting)
    //            {
    //                bee.SetDestination(transform.position);
    //                bee.StateMachine.ChangeState(bee.chaseState);
    //            }
    //            processed++;
    //        }
    //        batchTracker = (batchTracker + processed) % count;
    //    }
    //}

    public void BuyBee(GameObject bee, Transform spawnPosition)
    {
        if (bee == null) Debug.Log("Noo bee object");
        GameObject newBee = Instantiate(bee);
        BasicBee basicBee = newBee.GetComponent<BasicBee>();
        basicBee.homeCoordinates = spawnPosition.position;
        basicBee.SetMyParent(this);
        allBees.Add(basicBee);
        PlayerBeeSaved newSavedBee = new PlayerBeeSaved(basicBee, newBee);
        Debug.Log("Saving bee Data");
        savedBees.Add(newSavedBee);
        Debug.Log("Saved bees count = " + savedBees.Count);
        newBee.transform.position = spawnPosition.position;

    }

    // Controling bees

    public void SetBeeStatesToFollow(BasicBee orderedBee)
    {
        Game_Manager.instance.Bee_FollowPlayer(orderedBee, true);

    }
    public void FocusTargetedEnemy()
    {

    }
    public void MoveToTargetedSpot()
    {

    }
    public void StartComandingBees()
    {

    }
    public void StopComandingBees() { }
    public void FollowTarget(BasicBee bee, Transform target)
    {

    }
    public List<BasicBee> DepositPollin()
    {
        isConvertingPollen = true;
        foreach (var bee in allBees)
        {
            bee.playerComand = true;
            bee.SetDestination(bee.homeCoordinates, false);
            Debug.Log($"Bee destination set to {bee.homeCoordinates}");

        }

        return allBees;
    }
    public void CleareComands()
    {
        foreach (var bee in allBees)
        {
            bee.playerComand = false;
        }
    }
    #endregion
    #region Shop and Upgrade Fuctions
    public void AddCell()
    {
        ownedCellsAmount++;
    }
    /// <summary>
    /// Upgrades pollin storage level and recalculates maxPollinStorage using:
    ///   max = base * (pollinStorageMULTI)^(level-1)
    /// This computes from the base each time to avoid accumulated floating-point drift.
    /// </summary>
    public void UpgradeMaxPollinStorage()
    {
        // increment level
        pollinStorageLevel++;

        // exponent: level 1 -> exponent 0 (gives base), level 2 -> exponent 1, etc.
        double exponent = Math.Max(0, pollinStorageLevel - 1);

        // compute as double for numeric stability
        double pow = Math.Pow((double)pollinStorageMULTI, exponent);
        double newMaxDouble = (double)_baseMaxPollinStorage * pow;

        // clamp / guard against NaN/Infinity and overflow beyond long.MaxValue
        if (double.IsNaN(newMaxDouble) || double.IsInfinity(newMaxDouble) || newMaxDouble >= (double)long.MaxValue)
        {
            maxPollinStorage = long.MaxValue;
        }
        else
        {
            // round to nearest long (you can use Math.Floor or cast for truncation if you prefer)
            long rounded = (long)Math.Round(newMaxDouble);
            // also ensure it is >= 1
            maxPollinStorage = Math.Max(1L, rounded);
        }

        // update UI text (adjust to your exact UI field if its name differs)
        visualsUI.pollinCounterText.text = $"Pollin: {currentPollinAmount}/{maxPollinStorage}";
    }

    // Optional helper: compute required pollinStorageMULTI to reach 'target' from base in 'levels' upgrades.
    // Example usage: pollinStorageMULTI = ComputeMultiplierForTarget(20_000_000, 50);
    public float ComputeMultiplierForTarget(long targetMax, int levels)
    {
        if (levels <= 0 || _baseMaxPollinStorage <= 0) return pollinStorageMULTI;
        // We want base * r^(levels) = target  if levels counts upgrades from base,
        // but since our exponent uses (level-1) in the implementation, choose semantics you want.
        // Here we assume: starting at level 1 (base), after `levels` upgrades (i.e. level 1->level 1+levels)
        // the exponent should be `levels`.
        double r = Math.Pow((double)targetMax / _baseMaxPollinStorage, 1.0 / levels);
        if (double.IsNaN(r) || double.IsInfinity(r)) return pollinStorageMULTI;
        return (float)r;
    }

    public void AddFoodItem(BeeFood food)
    {
        if (foodStorage.ContainsKey(food))
        {
            foodStorage[food]++;
        }
        else
        {
            foodStorage.Add(food, 1);
        }
    }
    #endregion
}
public class PlayerBeeSaved
{
    public BasicBee beeScritp;
    public GameObject beeObject;
    // List<Mutations> beeMutations = new Lsit<Mutation>();
    public PlayerBeeSaved(BasicBee bee, GameObject objectOfBee)
    {
        beeScritp = bee;
        beeObject = objectOfBee;
    }
}
