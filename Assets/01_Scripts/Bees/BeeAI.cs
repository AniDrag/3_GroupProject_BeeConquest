using TMPro;
using UnityEditor.Playables;
using UnityEngine;
using static BeeCore;
[RequireComponent(typeof(BeeStateMachine))]
public class BeeAI : Stats
{
    // ───────────── ENUMS ─────────────
    public enum BeeAttribute { Red, Blue, Green, Dark, Light }
    public enum BeeState { Idle, Moving, Collecting, Attacking, Following }
    //public enum BeeClass { Worker, Attacker, Defender }

    // ───────────── FIELDS ─────────────
    [Header("--------------- Runtime ---------------")]
    public BeeAttribute beeAttribute;
    public BeeState beeState;
    //public BeeClass beeClass;

    [Header("--------------- Settings ---------------")]
    [SerializeField, Range(1, 100)] public float collectionStrength { get; private set; } = 5f;
    [SerializeField, Range(0.1f, 5f)] public float heightOffsetY { get; private set; } = 0.4f;
    [SerializeField, Range(0.1f, 5f)] public float collectionSpeed { get; private set; } = 1;
    [SerializeField] public float speed { get; private set; } = 3; // levl * speedScale

    // Who commanded me last? (server / player)
    public bool PlayerOverride { get; private set; }

    //-------------------
    //      Bee Info
    //-------------------
    public PlayerCore player;//{ get; private set; }
    public int parentID { get; private set; } = 0;

    // -------------------
    // Target management
    // -------------------
    public Vector3 targetPosition { get; private set; }
    public EnemyCore TargetEnemy { get; private set; }
    public FieldCell TargetField { get; private set; }
    //-------------------
    //      Boleans Info
    //-------------------
    public bool atDestination;
    public bool getingPollin;// means we are standing on a field
    public bool recivedPlayerOrder;

    //-------------------
    //      floats Info
    //-------------------
    public float getTravelingTime { get; private set; }

    //-------------------
    //      Stat Info
    //-------------------
    #region Stats and scalers for stats
    private long currentXP; // when killing enemys and collecting polin mybe minigames and consumable items
    private float currentStamina;
    private DamageData damage;

    // Ability slot

    [Header("--------------- Stat modifiers ---------------")]
    [SerializeField] private int staminaScaler = 1;
    [SerializeField] private int damageScaler = 1;
    [SerializeField] private int speedScaler = 1;
    [Tooltip("on max level how much durability it can consume from a flower")]
    [SerializeField,Range(10,100)] private int collectionCap = 20;
    [SerializeField] private float critDamage = 1;
    [SerializeField] private int critChance = 1;
    #endregion
  

    //-------------------
    //      States and state macine Info
    //-------------------
    #region State machine Vars
    [SerializeField] private BeeStateMachine stateMachine;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Game_Manager.instance.players.ContainsKey(parentID)) 
         Game_Manager.instance.players[parentID].playerBeesTwo.Add(this); // TEMP ADDING ALL BEES TO ONE PLAYER (THE CURRENT ONE)!!
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
        int setNewDamage = Strength * CharacterLevel;
        damage = new DamageData(setNewDamage, DamageType.Physical, critDamage);

        //-------------------
        //      States initialization
        //-------------------
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

            getingPollin = player.currentField != null;
            if (recivedPlayerOrder) atDestination = Vector3.Distance(transform.position, player.transform.position) < 2f;
            else atDestination = Vector3.Distance(transform.position, targetPosition) < 0.1f;

            if (atDestination)
            {
                if (recivedPlayerOrder) recivedPlayerOrder = false;
                else GetDestinationData();
                return;
            }


        }
    }

    
    public void SetDestination(Vector3 newDestination)
    {
        targetPosition = Vector3.zero;
        targetPosition = newDestination + new Vector3(Random.Range(-0.5f, 0.5f), heightOffsetY, Random.Range(-0.5f, 0.5f));
        Debug.Log(targetPosition);
        getTravelingTime = GetTravelTime(targetPosition) + Time.time;
    }
    private float GetTravelTime(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        return distance / speed; // seconds
    }
   //public void PING_DestinationNearPlayer()
   //{
   //    //Debug.Log("Pinging a destination around player");
   //    //Game_Manager.instance.BeeMovementRequest(this);
   //
   //    // send a ping to server
   //}
   //public void PING_DestinationToFieldCell()
   //{
   //    //Debug.Log("Pinging a destination to a field cell");
   //    // send a ping to server
   //}
   //public void PING_DestinationToEnemy()
   //{
   //   // Debug.Log("Pinging a destination to an enemy ai");
   //}
   public void PING_CatchPlayer()
    {
       // Debug.Log("Pinging a destination to catch player");
        //SetDestination(Game_Manager.instance.players[playerID].target.position);
        //Debug.Log("I HAVE TO CATCH YOU");
        //AI();
    }

    public void SetMyParent(PlayerCore parentPlayer)
    {
        player = parentPlayer;
        parentID = parentPlayer.playerID;
    }
    public void GetDestinationData()
    {
        if (TargetEnemy != null)
        {
            //Game_Manager.instance.BeeCollectionRequest(this);
        }
        else if (getingPollin)
        {
           // Debug.Log("Pinging a destination to a field cell");
            Game_Manager.instance.BEE_PollinCollectionRequest(this, GetTravelTime(targetPosition) +1); // you'll implement this
        }
        else
        {
           // Debug.Log("Pinging a destination to a random location neare player cell");
            // Idle or moving near player
            Game_Manager.instance.BEE_IdleMoveRequest(this);
        }
    }

    public void PlayerRequest()
    {
        recivedPlayerOrder = true;
    }

   private void OnDrawGizmos()
   {
       Gizmos.color = Color.yellow;
       Gizmos.DrawSphere(targetPosition, 0.3f);
       //Gizmos.DrawSphere(player.transform.position, 0.3f);
   }
}
