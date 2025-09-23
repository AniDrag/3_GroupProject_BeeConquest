using AniDrag.Utility;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
[RequireComponent(typeof(BeeStateMachine))]
public class BasicBee : Stats
{
    #region ───────────── BEE STATS ─────────────
    // ───────────── General Bee Info ─────────────
    [Header(" ───────────── General Bee Info ─────────────")]
    public string BeeName { get; private set; } = "Name";
    public Sprite BeeSprite;
    public ColorAtribute beeAtribute { get; private set; }
    public BeeState beeState;
    [Header(" ───────────── Refrences ─────────────")]
    public BeeStateMachine StateMachine;


    [Header(" ───────────── Base Stats ─────────────")]
    [SerializeField] private int beeVitality = 1;
    [SerializeField] private int beeStrength = 1;
    [SerializeField] private int beeDexterity = 1;
    [SerializeField] private int beeAgility = 1;
    [SerializeField] private int beeLuck = 1;

    [Header(" ───────────── Base Stats increse per level ─────────────")]
    [SerializeField] private int vitIncrease = 1;
    [SerializeField] private int strIncrease = 1;
    [SerializeField] private int dexIncrease = 1;
    [SerializeField] private int agiIncrease = 1;
    [SerializeField] private int lucIncrease = 1;
    [SerializeField] private int statpointIncrese = 3;

    [Header(" ───────────── On level up multipliers ─────────────")]
    [SerializeField] private float beeStaminaMulti = 1;
    //[SerializeField] private float beeLevelUpMulti = 2;

    [Header(" ───────────── Bee Stats ─────────────")]
    private float beeSpeed = 1;
    [SerializeField, Range(10, 100)] private int flowerDurabilityCap = 20; // collection strenght cant go above this
    private int flowerDurabilityDamage = 5;// Durability damage to flowers
    private float pollinCollectionSpeed = 5; // Time in s before bee collects Pollin
    public int statPoints;
    
    [Header(" ───────────── Bee Ability details ─────────────")]
    [Range(0.01f, 1f)] public float spawnTokenChance = 0.1f;
    public AbilitySettings beeAbility;

    [Header(" ───────────── Bee ModifiedStats Stats ─────────────")]
    public float modedBeeSpeed;
    public int modedFlowerDurabilityDamage;// Durability damage to flowers
    public float modedPollinCollectionSpeed; // Time in s before bee collects Pollin
    public float modedSpawnTokenChance;
    #endregion
    #region ───────────── PLAYER DATA ─────────────
    [Header(" ───────────── Player Info ─────────────")]
    public PlayerCore player;
    public int playerID { get; private set; } = 0;
    public bool playerComand;
    #endregion
    #region ───────────── STATE MACHINE DATA ─────────────
    // enum BeeState { Idle, Moving, Collecting, Attacking, Following }

    public BeeIdleState idleState;
    public BeeMoveToTargetState moveingState;
    public BeeChasePlayerState chaseState;
    public BeeCollectingPolinState pollinCollectionState;
    public BeeCombatState combatState;

    #endregion
    #region ───────────── MOVEMENT DATA ─────────────
    [Header(" ───────────── Movemant offsets ─────────────")]
    [SerializeField, Range(.1f, 1f)] private float heightOffsetY = 0.4f;
    [SerializeField, Range(1, 10)] private float stopBeforeTarget = 0.5f;
    [SerializeField, Range(1, 10)] private float stopBeforePlayerReached = 3f;


    [Header(" ───────────── Movemant data ─────────────")]
    public Vector3 destinationPoint { get; private set; }

    public Vector3 homeCoordinates;// Add
    public bool atDestination { get; private set; }
    public EnemyCore TargetEnemy { get; private set; }
    public FieldCell TargetField { get; private set; }
    public Transform TargetComand { get; private set; }
    private List<float> xOffset = new List<float>();
    private List<float> zOffset = new List<float>();
    int offsetIndex = 0;
    #endregion
    #region ───────────── TICK & DISTANCE ─────────────
    [SerializeField, Range(1, 10)] float normalBeeTickSpeed = 1.5f;
    [SerializeField, Range(1, 10)] float followPlayerTickSpeed = 1.5f;
    public float getTravelingTime { get; private set; }
    private float beeStateUpdateInterval = 0;
    private float beeStateTimer = 0f;
    private float beeNextStateTime = 0f;
    public long _curentXP = 0;


    //[SerializeField, Range(.001f, .1f)] private float moveaAnimUpdateTimer = 0.05f;
    private float moveaAnimTimer = 0f;
    //private float moveaAnimNextTime = 0f;
    #endregion
    #region UNITY FUNCTION
    private void Awake()
    {
        beeStateUpdateInterval = normalBeeTickSpeed;
        GenerateOffsets();
        //-------------------
        //      States initialization
        //-------------------
        if (StateMachine == null)
            StateMachine = GetComponent<BeeStateMachine>();
        if (StateMachine == null)
            StateMachine = gameObject.AddComponent<BeeStateMachine>();
        if (StateMachine == null) Debug.LogWarning("No stte machine");
        

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
    protected virtual void Start()
    {
        //-------------------
        //      Stat initialization
        //-------------------
        SetBaseStats(beeVitality, beeStrength, beeDexterity, beeAgility, beeLuck);
        SetMultipliers(1, 1, beeStaminaMulti);
        SetLevel(1);
        //Debug.Log($"Agility={Agility}, Level={CharacterLevel}, beeSpeed={beeSpeed}");

        UpdateBeeStats();
        if (player == null) Debug.LogWarning("I have no player parent");
    }

    private void Update()
    {
        StateMachine.currentState.LogicUpdate();
    }
    private void FixedUpdate()
    {
        // ───── STATE MACHINE TICK RATE ─────
        beeStateTimer += Time.fixedDeltaTime;
        if (beeStateTimer >= beeNextStateTime)
        {
            beeStateTimer = 0f;
            beeNextStateTime = Mathf.Max(0.3f, beeStateUpdateInterval);
            StateMachine.currentState.FixedLogicUpdate();
            UpdateAtDestination();
        }
        // ───── MOVEMENT ANIMATION TICK RATE ─────
        if (beeState != BeeState.Following && beeState != BeeState.Moving) return;
        moveaAnimTimer += Time.fixedDeltaTime;
        SmoothMove(destinationPoint);
    }
    #endregion
    #region BEE MOVEMENT FUNCTIONS
    private void SmoothMove(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, modedBeeSpeed * Time.fixedDeltaTime);
    }
    private void UpdateAtDestination()
    {
        float distance = Vector3.Distance(transform.position, destinationPoint);
        float tolerance = beeState == BeeState.Following ? stopBeforePlayerReached : stopBeforeTarget;
        atDestination = distance <= tolerance;

        if (atDestination) // only when we transition to arrived
        {
            beeStateUpdateInterval = 1.5f;
            //Debug.Log($"[ARRIVED] Actual arrival time: {Time.time:F3}. Expected: {expectedArrivalTime:F3}. Delta: {Time.time - expectedArrivalTime:F3}s");
        }
        else if (!atDestination)
        {
            beeStateUpdateInterval = (distance - tolerance) / modedBeeSpeed;
            getTravelingTime = beeState == BeeState.Following && followPlayerTickSpeed < beeStateUpdateInterval? followPlayerTickSpeed: beeStateUpdateInterval;
        }
    }
    public void SetDestination(Vector3 newDestination)
    {
        destinationPoint = newDestination + RandomOffset();// Save data and have a random 10 offsets that we can roatate from
        atDestination = false;
        transform.LookAt(destinationPoint);
    }
    Vector3 RandomOffset()
    {
        float x = xOffset[offsetIndex];
        float z = zOffset[offsetIndex];
        if (offsetIndex < 9) offsetIndex++;
        else offsetIndex = 0;
        return new Vector3(x, heightOffsetY, z);

    }
    public float GetTravelTime(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        float effectiveSpeed = Mathf.Max(0.0001f, modedBeeSpeed); // avoid divide-by-zero
        float travelTime = distance / effectiveSpeed;     // seconds

        //Debug.Log($"[ETA] pos:{transform.position:F3} -> dest:{destination:F3} dist:{distance:F3} " + $"speed:{effectiveSpeed:F3} travel:{travelTime:F3}s");
        return travelTime;
    }
    public float GetPollinCollectionTime(Vector3 moveToPoint)
    {
        return GetTravelTime(moveToPoint) + pollinCollectionSpeed;
    }
    #endregion
    #region SERVER REQUEST FUNCTIONS
    public void GetDestinationData()
    {
         if (player.currentField != null) Game_Manager.instance.Bee_CellRequest(this);
         else Game_Manager.instance.Bee_IdleMove(this);
    }
    #endregion
    #region PLAYER COMUNICATION FUNCTIONS
    public virtual void ReciveComand(Vector3 moveToPoint)
    {


    }
    public void SetMyParent(PlayerCore parentPlayer)
    {
        player = parentPlayer;
        playerID = parentPlayer.playerID;
    }
    #endregion
    #region ABILITY RELATED LOGIC
    public virtual void TriggerAbilityLogic(BasicBee bee, PlayerCore player, Vector3 origin)
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
    #region STAT RELATED FUNCTIONS
    void UpdateBeeStats()
    {
        spawnTokenChance = .1f + beeLuck / 100f;
        beeSpeed = 3 + (Agility * CharacterLevel) / 10f;
        flowerDurabilityDamage = Mathf.RoundToInt(Mathf.Min(flowerDurabilityCap, Strength * CharacterLevel + Dexterity / 2f));
        pollinCollectionSpeed = Mathf.Max(1, 5 - ((Agility * CharacterLevel) / 100f));
        //Debug.Log(GetModifiedStat(StatType.Speed, beeSpeed));
        modedBeeSpeed = GetModifiedStat(StatType.Speed, beeSpeed);
        modedFlowerDurabilityDamage = (int)GetModifiedStat(StatType.CollectionStrength, flowerDurabilityDamage);
        modedPollinCollectionSpeed = GetModifiedStat(StatType.CollectionSpeed, pollinCollectionSpeed);
        modedSpawnTokenChance = GetModifiedStat(StatType.SpawnTokenChance, spawnTokenChance);
        //Debug.Log($"modedBeeSpeed after buffs={modedBeeSpeed}");
    }
    public override void LevelUp()
    {
        base.LevelUp();

        UpdateStats();

        if (CharacterLevel % 4 == 0)
            maxXP *= 4;
        else
            maxXP *= 2;
    }
    public void StatIncrese(StatType type)
    {
        switch(type)
        {
            case StatType.Vitality: beeVitality++; break;
            case StatType.Strength: beeStrength++; break;
            case StatType.Dexterity: beeDexterity++; break;
            case StatType.Agility: beeAgility++; break;
            case StatType.Luck: beeLuck++; break;
        }
        UpdateStats();
    }
    public override void UpdateStats()
    {
        beeVitality += vitIncrease;
        beeStrength += strIncrease;
        beeDexterity += dexIncrease;
        beeAgility += agiIncrease;
        beeLuck += lucIncrease;
        //Debug.Log($"Agility={Agility}, Level={CharacterLevel}, raw beeSpeed={beeSpeed}");
        //Debug.Log($"modedBeeSpeed before buffs={modedBeeSpeed}");
        UpdateBeeStats();
       
        //When bee levels up
    }
    public void AddXP(long amount)
    {
        _curentXP += amount;
        if(_curentXP > maxXP)
        {
            _curentXP -= maxXP;
            RepetedLevelUp(_curentXP);
        }
    }
    private void RepetedLevelUp(long amount)
    {
        statPoints += statpointIncrese;
        if (_curentXP > maxXP)
        {
            _curentXP -= maxXP;
            LevelUp();
            RepetedLevelUp(_curentXP);
        }
    }
    #endregion
    #region OTHER FUNCTIONS
    public int XpToLevelUP => maxXP;
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

    private void GenerateOffsets()
    {
        for (int i = 0; i < 10; i++)
        {
            xOffset.Add(Random.Range(-stopBeforeTarget, stopBeforeTarget));
            zOffset.Add(Random.Range(-stopBeforeTarget, stopBeforeTarget));
        }
    }
    #endregion
}
