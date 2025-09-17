using AniDrag.Utility;
using System.Collections.Generic;
using UnityEngine;
public class PlayerCore : MonoBehaviour
{
    [Header("----- UI refrences -----")]
    [SerializeField] public UI_Visuals visualsUI;
    [SerializeField] public UI_Dialogue dialogueUI;
    [SerializeField] public UI_Quests questUI;


    [Header("----- Bee Data -----")]
    public bool showBeeData;
    [SerializeField,Range(5,25),ShowIf("showBeeData")] int startFollowDistance = 5;
    [SerializeField, ShowIf("showBeeData")] int spawnNumPerClick = 75;
    [SerializeField, ShowIf("showBeeData")] public BeeAI[] testBee;
    [ShowIf("showBeeData")] private Dictionary<int, List<BeeAI>> beeGroups = new Dictionary<int, List<BeeAI>>();// this is a dictionary of 3 squads of warrior and defender bees the player can asign adn then manipulate
    [ShowIf("showBeeData")] public List<BeeAI> playerBees { get; private set; } = new List<BeeAI>();
    [SerializeField, ShowIf("showBeeData")] GameObject BeePRF;

    [Header("----- Field Data -----")]
    public FieldGenerator currentField {  get; private set; }


    [Header("----- Inventory Data -----")]
    [SerializeField] private long polinStorage = 0;
    [SerializeField] private long honeyStorage = 0;
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
            bee.SetMyParent(this);
            playerBees.Add(bee);
            
        }
        beeGroups.Add(1, playerBees);

        PlayerServerData data = new PlayerServerData(playerID, transform, this, playerBees) { };
        Game_Manager.instance.JoinServer(playerID, data);

    }
    void Start()
    {
        visualsUI.pollinCounterText.text = $"Pollin: {polinStorage}/{maxPollinStorage}";
        visualsUI.honeyCounterText.text = $"Nicterial: {honeyStorage}";
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

    private float perSecond = 10;
    private float polinPerSecTime = 0f;
    private float pollinPerSecRareTime = 0f;
    private long oldPollinAmount = 0;
    private void FixedUpdate()
    {
        //if (playerBees.Count > 0) Debug.Log(playerBees.Count + " Bee amount from Player");
        for (int i = 0; i < playerBees.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, playerBees[i].transform.position);
            if (distance >= startFollowDistance && 
                playerBees[i].StateMachine.currentState != playerBees[i].pollinCollectionState && 
                playerBees[i].StateMachine.currentState != playerBees[i].combatState)
            {
                //Debug.Log("player requested bee to follow DISTANCE:" + distance);
                Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(playerBees[i]);
            }

        }

        playerRareTimer += Time.fixedDeltaTime;
        polinPerSecTime += Time.fixedDeltaTime;
        if (playerRareTimer >= playerNextRareTime)
        {
            playerRareTimer = 0f;
            playerNextRareTime = Mathf.Max(0.01f, playerStateUpdateInterval);
            long honeySum = 0;
            while(honeyQueue.TryDequeue(out var val))
            {
                honeySum += val;
            }
            ActuallyShowHoneyVisual(honeySum);
        }
        if (polinPerSecTime >= pollinPerSecRareTime)
        {
            //Debug.Log("Pollin per sec update");
            polinPerSecTime = 0f;
            pollinPerSecRareTime = Mathf.Max(0.01f, perSecond);
            float perSec = (polinStorage - oldPollinAmount) / perSecond;
            if(perSec < 0) perSec = 0;
            visualsUI.pollinPerSecText.text = $"{perSec:F1}/s";
            oldPollinAmount = polinStorage;
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
            visualsUI.pollinCounterText.text = $"Pollin: {polinStorage}/{maxPollinStorage}";
            visualsUI.UI_UpdatePollin(polinStorage, maxPollinStorage);
            return tempNum;
        }
        else
        {
            polinStorage -= amount;
            visualsUI.pollinCounterText.text = $"Pollin: {polinStorage}/{maxPollinStorage}";
            visualsUI.UI_UpdatePollin(polinStorage, maxPollinStorage);
            return amount;
        }
        
    }
    public void AddPollin(long pollen, long honey)
    {
        polinStorage += pollen;
        if(polinStorage >= maxPollinStorage)
        {
            polinStorage = maxPollinStorage;
            RemoveField();
        }
        visualsUI.pollinCounterText.text = $"Pollin: {polinStorage}/{maxPollinStorage}";
        visualsUI.UI_UpdatePollin(polinStorage,maxPollinStorage);
    }
    public void AddHoney(long amount)
    {
        honeyStorage += amount;
        ShowHoneyVisual(amount);
        visualsUI.pollinCounterText.text = $"Pollin: {polinStorage}/{maxPollinStorage}";
        visualsUI.honeyCounterText.text = $"Nicterial: {honeyStorage}";
        visualsUI.UI_UpdatePollin(polinStorage, maxPollinStorage);
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

    #region Field Associated Functions
    public void AsignField(FieldGenerator newField)
    {// by not having a field asigned we bees will not collect pollin
        if (polinStorage >= maxPollinStorage) return;

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
