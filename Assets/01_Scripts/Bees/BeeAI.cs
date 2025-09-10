using TMPro;
using UnityEditor.Playables;
using UnityEngine;
public enum BeeStates { Idle, Moving, Collecting, Combat }
public class BeeAI : Stats
{
   

    // Who commanded me last? (server / player)
    public bool PlayerOverride { get; private set; }

    //-------------------
    //      Bee Info
    //-------------------
    public BeeStates state;
    public PlayerCore player;//{ get; private set; }
    public int parentID { get; private set; }

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
    [SerializeField] float heightOffsetY = 1;
    public float getTravelingTime { get; private set; }

    //-------------------
    //      Stat Info
    //-------------------
    #region Stats and scalers for stats
    private long currentXP; // when killing enemys and collecting polin mybe minigames and consumable items
    public float speed { get; private set; } = 3; // levl * speedScale
    private float currentStamina;
    public float collectionStrenght { get; private set; } = 5;// levl * collectionScale
    private float collectionSpeed; // levl * speedScale
    private DamageData damage;

    // Ability slot

    // Scalers
    [SerializeField] private int staminaScaler = 1;
    [SerializeField] private int damageScaler = 1;
    [SerializeField] private int speedScaler = 1;
    [Tooltip("on max level how much durability it can consume from a flower")]
    //[SerializeField,Range(10,100)] private int collectionCap = 20;
    [SerializeField] private float critDamage = 1;
    //[SerializeField] private int critChance = 1;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Game_Manager.instance.players.ContainsKey(parentID)) 
         Game_Manager.instance.players[parentID].playerBeesTwo.Add(this); // TEMP ADDING ALL BEES TO ONE PLAYER (THE CURRENT ONE)!!

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
        stateMachine.currentState.FixedLogicUpdate();

        // calculate distance only when bee is supposed to be at destinaion
        if (Time.time <= getTravelingTime) return;
        
        if (recivedPlayerOrder) atDestination = Vector3.Distance(transform.position, player.transform.position) < 2f;
        else atDestination = Vector3.Distance(transform.position, targetPosition) < 0.1f;

        if (atDestination)
        { 
            if(recivedPlayerOrder) recivedPlayerOrder = false;
            else GetDestinationData();
            return;
        }

        getTravelingTime = GetTravelTime(targetPosition);
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
            Game_Manager.instance.BEE_PollinCollectionRequest(this); // you'll implement this
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
