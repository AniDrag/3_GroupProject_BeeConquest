using TMPro;
using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;
[RequireComponent(typeof(BeeStateMachine))]
public class BeeAI : Stats
{
    // ───────────── ENUMS ─────────────
    #region Enums 
    public enum BeeAttribute { Red, Blue, Green, Dark, Light }
    public enum BeeState { Idle, Moving, Collecting, Attacking, Following }
    //public enum BeeClass { Worker, Attacker, Defender }
    #endregion

    // ───────────── TARGETS ─────────────
    #region Targets
    public Vector3 destinationPoint { get; private set; }
    public EnemyCore TargetEnemy { get; private set; }
    public FieldCell TargetField { get; private set; }
    public bool atDestination { get; private set; }
    #endregion
    // ───────────── PARENT INFO ─────────────
    public PlayerCore player;//{ get; private set; }
    public int playerID { get; private set; } = 0;
    public bool playerComand { get; private set; }

    // ───────────── FIELDS ─────────────
    #region Fields
    [Header("--------------- Runtime ---------------")]
    public BeeAttribute beeAttribute;
    public BeeState beeState;
    //public BeeClass beeClass;

    [Header("--------------- Settings ---------------")]
    [SerializeField, Range(1, 100)] public float collectionStrength { get; private set; } = 40f;
    [SerializeField, Range(0.1f, 5f)] public float heightOffsetY { get; private set; } = 0.4f;
    [SerializeField, Range(0.1f, 5f)] public float collectionSpeed { get; private set; } = 1;
    [SerializeField] public float speed { get; private set; } = 3; // levl * speedScale

    [Header("--------------- Stat modifiers ---------------")]
    [SerializeField] private int staminaScaler = 1;
    [SerializeField] private int damageScaler = 1;
    [SerializeField] private int speedScaler = 1;
    [Tooltip("on max level how much durability it can consume from a flower")]
    [SerializeField, Range(10, 100)] private int collectionCap = 20;
    [SerializeField] private float critDamage = 1;
    [SerializeField] private int critChance = 1;

    #endregion


    #region Properties
    //-------------------
    //      Stat Info
    //-------------------
   
    private long currentXP; // when killing enemys and collecting polin mybe minigames and consumable items
    private float currentStamina;
    public DamageData damage { get; private set; }
    public float getTravelingTime { get; private set; }

    // Ability slot


    #endregion


    //-------------------
    //      States and state macine Info
    //-------------------
    #region State machine Vars
    [SerializeField] public BeeStateMachine stateMachine { get; private set; }
    public BeeIdleState idleState;
    public BeeChasePlayerState chaseState;
    public BeeMoveToTargetState moveingState;
    public BeeCollectingPolinState pollinCollectionState;
    public BeeCombatState combatState;
    #endregion
    //-------------------
    //      Tick rate settings
    //-------------------
    private float fieldStateUpdateInterval =1.5f;
    private float fieldRareTimer = 0f;
    private float fieldNextRareTime = 0f;

    #region Default functions
    void Start()
    {
        //if (Game_Manager.instance.players.ContainsKey(playerID)) 
        // Game_Manager.instance.players[playerID].playerBeesTwo.Add(this); // TEMP ADDING ALL BEES TO ONE PLAYER (THE CURRENT ONE)!!
        SetMyParent(player);
        //-------------------
        //      Stat initialization
        //-------------------
        SetBaseStats(staminaScaler,damageScaler,1,speedScaler);
        SetMultipliers();
        //speed = Agility * CharacterLevel;
        //-------------------
        //      damage initialization
        //-------------------
        int setNewDamage = Strength * 5;
        damage = new DamageData(setNewDamage, DamageType.Physical, critDamage);

        //-------------------
        //      States initialization
        //-------------------
        stateMachine = transform.GetComponent<BeeStateMachine>();
        idleState = new BeeIdleState(stateMachine,this);
        chaseState = new BeeChasePlayerState(stateMachine, this);
        moveingState = new BeeMoveToTargetState(stateMachine, this);
        pollinCollectionState = new BeeCollectingPolinState(stateMachine, this);
        combatState = new BeeCombatState(stateMachine, this);

        //-------------------
        //      State machine initialization
        //-------------------
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.currentState.LogicUpdate();
    }
    private void LateUpdate()
    {
        stateMachine.currentState.LateLogicUpdate();
    }
    private void FixedUpdate()
    {
        fieldRareTimer += Time.fixedDeltaTime;
        if (fieldRareTimer >= fieldNextRareTime)
        {
            float dt = fieldRareTimer;
            fieldRareTimer = 0f;
            fieldNextRareTime = Mathf.Max(0.01f, fieldStateUpdateInterval);

            stateMachine.currentState.FixedLogicUpdate();

            atDestination = Vector3.Distance(transform.position, destinationPoint) < (beeState == BeeState.Following? 2 : 0.01f);

            if (atDestination && !playerComand)
            {
                fieldStateUpdateInterval = 1.5f;
                GetDestinationData();
            }
            else fieldStateUpdateInterval = GetTravelTime(destinationPoint);
        }
        SmoothMove(destinationPoint);
    }
    #endregion
    // ───────────── API call Function ─────────────
    #region API Functins
    public void SetDestination(Vector3 newDestination, bool addOffset = true)
    {
        destinationPoint = newDestination +
            new Vector3(Random.Range(-0.5f, 0.5f), addOffset? heightOffsetY:0, Random.Range(-0.5f, 0.5f));

        getTravelingTime = GetTravelTime(destinationPoint) + Time.time;
        atDestination = false;
    }
    #endregion
    private float GetTravelTime(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        return distance / speed; // seconds
    }
    public void SetMyParent(PlayerCore parentPlayer)
    {
        player = parentPlayer;
        playerID = parentPlayer.playerID;
    }
    public void GetDestinationData()
    {
        if (TargetEnemy != null)
        {
            //Game_Manager.instance.BeeCollectionRequest(this);
        }
        else if (player.currentField != null)
        {
           // Debug.Log("Pinging a destination to a field cell");
            Game_Manager.instance.BEE_PollinCollectionRequest(this, GetTravelTime(destinationPoint) +1); // you'll implement this
        }
        else
        {
           // Debug.Log("Pinging a destination to a random location neare player cell");
            // Idle or moving near player
            Game_Manager.instance.BEE_IdleMoveRequest(this);
        }
    }

    public void SmoothMove(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void OnDrawGizmos()
   {
       Gizmos.color = Color.yellow;
       Gizmos.DrawSphere(destinationPoint, 0.3f);
       //Gizmos.DrawSphere(player.transform.position, 0.3f);
   }
}
