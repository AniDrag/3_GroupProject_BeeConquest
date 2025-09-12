using UnityEngine;
using static UnityEditorInternal.ReorderableList;
[RequireComponent(typeof(BeeStateMachine))]
public class BeeAI : Stats
{
    #region ───────────── ENUMS ─────────────
    public enum BeeAttribute { Red, Blue, Green, Dark, Light }
    public enum BeeState { Idle, Moving, Collecting, Attacking, Following }
    #endregion


    #region  ───────────── TARGETS ─────────────
    public Vector3 destinationPoint { get; private set; }
    public EnemyCore TargetEnemy { get; private set; }
    public FieldCell TargetField { get; private set; }
    public bool atDestination { get; private set; }
    #endregion

    // ───────────── PARENT INFO ─────────────
    public PlayerCore player { get; private set; }
    public int playerID { get; private set; } = 0;
    public bool playerComand { get; private set; }


    #region ───────────── SETTINGS ─────────────
    [Header("Bee Stats")]
    public BeeAttribute beeAttribute;
    public BeeState beeState;
    [SerializeField] public float speed = 3f;
    [SerializeField, Range(1, 100)] public float collectionStrength = 40f;
    [SerializeField, Range(0.1f, 5f)] public float heightOffsetY = 0.4f;
    [SerializeField, Range(0.1f, 5f)] public float collectionSpeed = 1f;
    [Tooltip("on max level how much durability it can consume from a flower")]
    //[SerializeField, Range(10, 100)] private int collectionCap = 20;
    [SerializeField] private float critDamage = 1;
    //[SerializeField] private int critChance = 1;

    [Header("Stat Modifiers")]
    //[SerializeField] private int staminaScaler = 1;
    //[SerializeField] private int damageScaler = 1;
    //[SerializeField] private int speedScaler = 1;

    // ───────────── PRIVATE STATS ─────────────
   //private long currentXP; 
   //private float currentStamina;

    // ───────────── READ ONLY STATS ─────────────
    public DamageData damage { get; private set; }
    public float getTravelingTime { get; private set; }

    // ─────────────ABILITIES ─────────────
    #endregion

        
    #region  ───────────── STATE MACHINE ─────────────
    public BeeStateMachine stateMachine { get; private set; }
    public BeeIdleState idleState;
    public BeeMoveToTargetState moveingState;
    public BeeChasePlayerState chaseState;
    public BeeCollectingPolinState pollinCollectionState;
    public BeeCombatState combatState;
    #endregion

    // ───────────── TICK & DISTANCE ─────────────
    private float beeStateUpdateInterval = 1.5f;
    private float beeRareTimer = 0f;
    private float beeNextRareTime = 0f;

    //      ───────────── DEFAULT UNITY FUNCTIONS ─────────────
    #region ───────────── DEFAULT UNITY FUNCTIONS ─────────────
    void Start()
    {
        if (player == null) Debug.LogWarning("I have no player parent");

        //-------------------
        //      damage initialization
        //-------------------
        int setNewDamage = 2 * 5;
        damage = new DamageData(setNewDamage, DamageType.Physical, critDamage);

        //-------------------
        //      States initialization
        //-------------------
        stateMachine = GetComponent<BeeStateMachine>();

        idleState = new BeeIdleState(stateMachine, this);
        chaseState = new BeeChasePlayerState(stateMachine, this);
        moveingState = new BeeMoveToTargetState(stateMachine, this);
        pollinCollectionState = new BeeCollectingPolinState(stateMachine, this);
        combatState = new BeeCombatState(stateMachine, this);

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
        // ───── FIELD TICK RATE ─────
        beeRareTimer += Time.fixedDeltaTime;
        if (beeRareTimer >= beeNextRareTime)
        {
            float dt = beeRareTimer;
            beeRareTimer = 0f;
            beeNextRareTime = Mathf.Max(0.3f,beeStateUpdateInterval);

            stateMachine.currentState.FixedLogicUpdate();
        }

        // ───── MOVE BEE ONLY IF NECESSARY ─────
        if (beeState == BeeState.Moving || beeState == BeeState.Following)
        {
            SmoothMove(destinationPoint);
            SmoothRotate(destinationPoint);


            // ───── CHECK ARRIVAL ─────
            UpdateAtDestination();
        }

        /*
        fieldRareTimer += Time.fixedDeltaTime;
        if (fieldRareTimer >= fieldNextRareTime)
        {
            float dt = fieldRareTimer;
            fieldRareTimer = 0f;
            


            stateMachine.currentState.FixedLogicUpdate();
            if (stateMachine.currentState != moveingState || stateMachine.currentState != chaseState){ fieldStateUpdateInterval = 1.5f; return; }


            fieldStateUpdateInterval = GetTravelTime(destinationPoint);
            fieldNextRareTime = Mathf.Max(0.1f, fieldStateUpdateInterval);
            float tolerance = beeState == BeeState.Following ? 2 : 0.01f;
            Debug.Log("Bee tolerance = "+tolerance);
            atDestination = Vector3.Distance(transform.position, destinationPoint) <= tolerance;
        }
        SmoothMove(destinationPoint);*/
    }
    #endregion

    //       ───────────── PUBLIC API ─────────────
    #region  ───────────── PUBLIC API ─────────────
    public void SetDestination(Vector3 newDestination, bool addOffset = true)
    {
        destinationPoint = newDestination +
            new Vector3(Random.Range(-0.5f, 0.5f), addOffset ? heightOffsetY : 0, Random.Range(-0.5f, 0.5f));
        atDestination = false;
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
            stateMachine.ChangeState(combatState);
        }
        else if (player.currentField != null)
        {
            Game_Manager.instance.BEE_PollinCollectionRequest(this, GetTravelTime(destinationPoint));
        }
        else
        {
            Game_Manager.instance.BEE_IdleMoveRequest(this);
        }
    }
    #endregion


    //      ───────────── HELPER FUNCTIONS ─────────────
    #region ───────────── HELPER FUNCTIONS ─────────────
    private void SmoothMove(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
    private void SmoothRotate(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
    private void UpdateAtDestination()
    {
        float distance = Vector3.Distance(transform.position, destinationPoint);
        float tolerance = (beeState == BeeState.Following) ? 2f : 0.2f;
        atDestination = distance <= tolerance;
        if (atDestination) 
            beeStateUpdateInterval = 1.5f;
        else 
                beeStateUpdateInterval = distance / speed;
    }
    private float GetTravelTime(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        return distance+ collectionSpeed;
    }
    #endregion


#if UNITY_EDITOR
    private void OnDrawGizmos()
   {
       Gizmos.color = Color.yellow;
       Gizmos.DrawSphere(destinationPoint, 0.3f);
       //Gizmos.DrawSphere(player.transform.position, 0.3f);
   }
#endif
}
