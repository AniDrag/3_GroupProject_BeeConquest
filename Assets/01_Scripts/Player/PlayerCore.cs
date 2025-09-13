using AniDrag.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class PlayerCore : MonoBehaviour
{
    [Header("----- UI refrences -----")]
    [SerializeField] private TMP_Text honneyStorageUI;
    [SerializeField] private TMP_Text polinStorageUI;
    [SerializeField] GameObject floatingNumPrefab;


    [Header("----- Bee Data -----")]
    [SerializeField] int spawnNumPerClick = 75;
    [SerializeField] public BeeAI[] testBee;
    private Dictionary<int, List<BeeAI>> beeGroups = new Dictionary<int, List<BeeAI>>();// this is a dictionary of 3 squads of warrior and defender bees the player can asign adn then manipulate
    private List<BeeAI> playerBees = new List<BeeAI>();
    [SerializeField] GameObject BeePRF;

    [Header("----- Field Data -----")]
    [SerializeField] public FieldGenerator currentField;


    [Header("----- Inventory Data -----")]
    [SerializeField] private long polinStorage;
    [SerializeField] private long honeyStorage;
    [SerializeField] private bool showReceivedHoney = true;
    [SerializeField] private bool showReceivedPollen = true;
    private long maxPollinStorage = 10000;
    private Queue<long> honeyQueue = new Queue<long>();

    #region Getters
    public int playerID { get; private set; } = 0;

    #endregion

    private void Awake()
    {

        foreach (var bee in testBee)
        {
            playerBees.Add(bee);
        }
        beeGroups.Add(1, playerBees);

        foreach (var bee in playerBees)
        {
            bee.SetMyParent(this);
        }

        PlayerServerData data = new PlayerServerData(playerID, transform, this, playerBees) { };

        Game_Manager.instance.JoinServer(playerID, data);
    }
    void Start()
    {
        for (int i = 0; i < spawnNumPerClick; i++)
        {
            GameObject newBee = Instantiate(BeePRF);
            BeeAI bee = newBee.GetComponent<BeeAI>();
            bee.SetMyParent(this);
            playerBees.Add(bee);
            Game_Manager.instance.players[playerID].playerBeesTwo.Add(bee);
            // bee nees a skin and a type
            //spawn bee and parent give the be the proper bee data and player data.
        }
    }

    private float playerStateUpdateInterval = .2f;
    private float playerRareTimer = 0f;
    private float playerNextRareTime = 0f;
    private void FixedUpdate()
    {
        //if (playerBees.Count > 0) Debug.Log(playerBees.Count + " Bee amount from Player");
        for (int i = 0; i < playerBees.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, playerBees[i].transform.position);
            if (distance > 5 && playerBees[i].stateMachine.currentState == playerBees[i].idleState)
            {
                //Debug.Log("player requested bee to follow DISTANCE:" + distance);
                Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(playerBees[i]);
            }

        }

        playerRareTimer += Time.fixedDeltaTime;
        if (playerRareTimer >= playerNextRareTime)
        {
            float dt = playerRareTimer;
            playerRareTimer = 0f;
            playerNextRareTime = Mathf.Max(0.01f, playerStateUpdateInterval);
            long honeySum = 0;
            while(honeyQueue.TryDequeue(out var val))
            {
                honeySum += val;
            }
            ActuallyShowHoneyVisual(honeySum);
        }
    }
    #region Collection and currency FUNCTIONS
    public long RemovePollin(long amount)
    {
        long tempNum = amount;
        if (amount > polinStorage)
        {
            tempNum = polinStorage;
            polinStorage = 0;
            return tempNum;
        }
        else
        {
            polinStorage -= amount;
            return amount;
        }
    }
    public void AddPollin(long pollen, long honey)
    {
        polinStorage += pollen;
        polinStorageUI.text = $"Pollin: {polinStorage}/{maxPollinStorage}";
    }
    public void AddHoney(long amount)
    {
        honeyStorage += amount;
        ShowHoneyVisual(amount);
        polinStorageUI.text = $"Pollin: {polinStorage}/{maxPollinStorage}";
        honneyStorageUI.text = $"Nicterial: {honeyStorage}";
    }
    public void ShowPollinVisual(long pollen, Vector3 position, CellColor color = CellColor.Red)
    {
        if (showReceivedPollen == true)
            FloatingLabelPool.Instance.ShowAmount(pollen, position, FloatingLabelPool.Instance.ColorForCell(color));
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

    #region Bee and Bee Controll Functions
    [Button("SpawnBees", ButtonSize.Medium, 0, 0, 0, 1, SdfIconType.None)]
    public void SpawnBees()
    {
        for (int i = 0; i < spawnNumPerClick; i++)
        {
            GameObject newBee = Instantiate(BeePRF);
            BeeAI bee = newBee.GetComponent<BeeAI>();
            bee.SetMyParent(this);
            playerBees.Add(bee);
            Game_Manager.instance.players[playerID].playerBeesTwo.Add(bee);
            // bee nees a skin and a type
            //spawn bee and parent give the be the proper bee data and player data.
        }
    }

    // Controling bees

    public void SetBeeStatesToFollow(BeeAI orderedBee)
    {
        Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(orderedBee, true);

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
    public void FollowTarget(BeeAI bee, Transform target) {

    }
    public void DepositPollin(Transform target)
    {
        foreach (var bee in playerBees)
        {
            bee.playerComand = true;
            bee.TargetComand = target;
            bee.SetDestination(target.position);
        }
    }
    public void CleareComands()
    {
        foreach (var bee in playerBees)
        {
            bee.playerComand = false;
        }
    }
    #endregion

}
