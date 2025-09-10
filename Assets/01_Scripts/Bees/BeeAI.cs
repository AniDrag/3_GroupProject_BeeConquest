using UnityEditor.Playables;
using UnityEngine;
public enum BeeStates { Idle, Moving, Collecting, Attacking }
public class BeeAI : MonoBehaviour
{
    public BeeStates state;
    public Transform player;
    public Vector3 moveToPoint;

    // some stats
    private string beeName;
    private int level;
    private long xpTolevelUP;
    private long currentXP; // when killing enemys and collecting polin mybe minigames and consumable items
    private float speed;// levl * speedScale
    private float speedScale;
    private float stamina;
    private float collectionStrenght;// levl * collectionScale
    private float collectionScaling; 
    private float collectionSpeed; // levl * speedScale
    private int damage;// level * scale = damage
    private int damageScaling;
    // Ability slot

    private EnemyCore enemy;
    private DamageData damage;

    #region State machine Vars
    [SerializeField] private BeeStateMachine stateMachine;
    public BeeIdleState idleState;
    public BeeChasePlayerState chaseState;
    public BeeMoveToTargetState moveingState;
    public BeeCombatState combatState;
    public BeeCollectingPolinState pollinCollectionState;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        idleState = new BeeIdleState(stateMachine,this);
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
        SmoothMoving();
    }

    private void SmoothMoving()
    {
       if (moveToPoint != Vector3.zero)
       {
           transform.position = Vector3.MoveTowards(transform.position, moveToPoint, Agility * Time.deltaTime);
       
       }
    }

}
