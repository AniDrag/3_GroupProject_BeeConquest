using UnityEngine;
public enum BeeStates { Idle, moving, collecting, attacking }
public class BeeAI : MonoBehaviour
{
    public BeeStates state;
    public Transform player;
    public Vector3 moveToPoint;

    
    private EnemyCore enemy;
    private DamageData damage;

    #region State machine Vars
    [SerializeField] private BeeStateMachine stateMachine;
    public BeeIdleState idleState;
   //public STATE_Walking walkingState;
   //public STATE_Running runningState;
   //public STATE_Jumping jumpingState;
   //public STATE_Crouching crouchingState;
   //public STATE_Falling fallingState;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        idleState = new BeeIdleState(stateMachine = this.stateMachine);
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
    }

    // Update is called once per frame

}
