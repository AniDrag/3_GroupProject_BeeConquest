using AniDrag.Utility;
using UnityEngine;
using UnityEngine.UI;
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
    public Transform TargetComand;
    public bool atDestination { get; private set; }
    #endregion

    // ───────────── PARENT INFO ─────────────
    public PlayerCore player { get; private set; }
    public int playerID { get; private set; } = 0;
    public bool playerComand;


    #region ───────────── SETTINGS ─────────────
    [Header("Bee Stats")]
    public BeeAttribute beeAttribute;
    public BeeState beeState;
    public float SpawnTokenChance { get; private set; }
    public float speed; //{ get; private set; }
    public float collectionStrength { get; private set; }
    public float heightOffsetY = 0.4f;
    public float collectionSpeed { get; private set; }
    [Tooltip("on max level how much durability it can consume from a flower")]
    [SerializeField] private float critDamage = 1;
    //[SerializeField, Range(10, 100)] private int collectionCap = 20;
    //[SerializeField] private int critChance = 1;
    

    [Header("Base Stats Modifiers")]
    [SerializeField] private int beeVitality = 1;
    [SerializeField] private int beeStrength = 1;
    [SerializeField] private int beeDexterity = 1;
    [SerializeField] private int beeAgility = 1;

    [Header("Base Stats Multiplayers")]
    [SerializeField] private float beeStaminaMulti = 1;

    // ───────────── PRIVATE STATS ─────────────
    //private long currentXP; 
    //private float currentStamina;

    // ───────────── READ ONLY STATS ─────────────
    public DamageData damage { get; private set; }
    public float getTravelingTime { get; private set; }


    // ─────────────ABILITIES ─────────────

    public AbilitySettings beeAbility;
    #endregion


    #region  ───────────── STATE MACHINE ─────────────
    public BeeStateMachine StateMachine;
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
    protected virtual void Start()
    {
        if (StateMachine == null)
            StateMachine = GetComponent<BeeStateMachine>();
        if (StateMachine == null)
            StateMachine = gameObject.AddComponent<BeeStateMachine>();
        if (StateMachine == null) Debug.LogWarning("No stte machine");
        if (player == null) Debug.LogWarning("I have no player parent");
        //-------------------
        //      Stat initialization
        //-------------------
        SetBaseStats(beeVitality, beeStrength, beeDexterity, beeAgility);
        SetMultipliers(1, 1, beeStaminaMulti);
        SetLevel(1);

        int setNewDamage = Strength * CharacterLevel + Dexterity/2;
        damage = new DamageData(setNewDamage, DamageType.Physical, critDamage);
        SpawnTokenChance = .1f;
        speed = Agility * CharacterLevel;
        collectionStrength = Strength * CharacterLevel;
        collectionSpeed =Mathf.Max(1,5-((Agility * CharacterLevel)/100f));

        //-------------------
        //      States initialization
        //-------------------

        idleState = new BeeIdleState(StateMachine, this);
        chaseState = new BeeChasePlayerState(StateMachine, this);
        moveingState = new BeeMoveToTargetState(StateMachine, this);
        pollinCollectionState = new BeeCollectingPolinState(StateMachine, this);
        combatState = new BeeCombatState(StateMachine, this);
        if (idleState == null) Debug.LogWarning("No idle state ");
        if (chaseState == null) Debug.LogWarning("No chase state ");
        if (moveingState == null) Debug.LogWarning("No moving state ");
        if (combatState == null) Debug.LogWarning("No combat State ");
        if (pollinCollectionState == null) Debug.LogWarning("No pollin Collection state ");


        StateMachine.Initialize(idleState);
    }

    private void Update()
    {
        StateMachine.currentState.LogicUpdate();
    }
    private void LateUpdate()
    {
      //  StateMachine.currentState.LateLogicUpdate();
    }
    private void FixedUpdate()
    {
        // ───── FIELD TICK RATE ─────
        beeRareTimer += Time.fixedDeltaTime;
        if (beeRareTimer >= beeNextRareTime)
        {
            beeRareTimer = 0f;
            beeNextRareTime = Mathf.Max(0.3f,beeStateUpdateInterval);

            StateMachine.currentState.FixedLogicUpdate();
        }

        // ───── MOVE BEE ONLY IF NECESSARY ─────
        if (beeState == BeeState.Moving || beeState == BeeState.Following)
        {
            SmoothMove(destinationPoint); 

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
    //private float expectedArrivalTime;
    public void SetDestination(Vector3 newDestination, bool addOffset = true)
    {
        destinationPoint = newDestination + new Vector3(Random.Range(-0.5f, 0.5f), addOffset ? heightOffsetY : 0, Random.Range(-0.5f, 0.5f));
        atDestination = false;
        transform.LookAt(destinationPoint);
        //float travelEta = GetTravelTime(destinationPoint);         // travel only
        //expectedArrivalTime = Time.time + travelEta;              // arrival moment
        //Debug.Log($"[SetDestination] Expected arrival in {travelEta:F3}s (at {expectedArrivalTime:F3}).");
    }

    public void SetMyParent(PlayerCore parentPlayer)
    {
        player = parentPlayer;
        playerID = parentPlayer.playerID;
    }

    public void GetDestinationData()
    {
        if (TargetEnemy != null) StateMachine.ChangeState(combatState);
        else if (player.currentField != null) Game_Manager.instance.BEE_PollinCollectionRequest(this);
        else Game_Manager.instance.BEE_IdleMoveRequest(this);
    }
    #endregion


    //      ───────────── HELPER FUNCTIONS ─────────────
    #region ───────────── HELPER FUNCTIONS ─────────────
    private void SmoothMove(Vector3 target)
    {
        //float newSpeed = GetModifiedStat(StatType.Speed,speed);
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);
    }
    private void UpdateAtDestination()
    {
        float distance = Vector3.Distance(transform.position, destinationPoint);
        float tolerance = beeState == BeeState.Following ? 2f : 0.2f;
        bool nowAtDestination = distance <= tolerance;

        if (nowAtDestination && !atDestination) // only when we transition to arrived
        {
            atDestination = true;
            //beeState = BeeState.Collecting;                     // or do stateMachine.ChangeState(pollinCollectionState)
            beeStateUpdateInterval = 1.5f;
            //Debug.Log($"[ARRIVED] Actual arrival time: {Time.time:F3}. Expected: {expectedArrivalTime:F3}. Delta: {Time.time - expectedArrivalTime:F3}s");

            // optionally: start collection state
            //StateMachine.ChangeState(pollinCollectionState);// Not supposed to change state inside Bee core!!!
        }
        else if (!nowAtDestination)
        {
            atDestination = false;
            beeStateUpdateInterval = distance / speed;
        }
    }
    public float CollectionDuration => collectionSpeed;

    // returns travel time in seconds (distance / speed) only
    public float GetTravelTime(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        float effectiveSpeed = Mathf.Max(0.0001f, speed); // avoid divide-by-zero
        float travelTime = distance / effectiveSpeed;     // seconds

        //Debug.Log($"[ETA] pos:{transform.position:F3} -> dest:{destination:F3} dist:{distance:F3} " + $"speed:{effectiveSpeed:F3} travel:{travelTime:F3}s");
        return travelTime;
    }

    
    #endregion

    #region Abilitie Logic
    public virtual void TriggerAbilityLogic(BeeAI bee, PlayerCore player, Vector3 origin)
    {

    }
    [Button]
    public void SpawnAbility()
    {
        GameObject ability = Instantiate(beeAbility.abilityVisualPrefab, transform.position, Quaternion.identity);
        ability.transform.GetChild(0).GetComponent<Image>().sprite = beeAbility.sprite;
        ability.GetComponent<Ability>().SetAbilityData(this, beeAbility.AbilityName);
    }
    #endregion

    public override void OnDeath()
    {
        base.OnDeath();
        // Return to base
        // Rest and dont collect pollin
    }
    public override void TakeDamage(DamageData data)
    {
        base.TakeDamage(data);
        //Deal damage to stamina / hp
    }
    public override void UpdateStats()
    {
        base.UpdateStats();
        //When bee levels up
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
   {
       Gizmos.color = Color.yellow;
       Gizmos.DrawSphere(destinationPoint, 0.3f);
       //Gizmos.DrawSphere(player.transform.position, 0.3f);
   }
#endif
}
