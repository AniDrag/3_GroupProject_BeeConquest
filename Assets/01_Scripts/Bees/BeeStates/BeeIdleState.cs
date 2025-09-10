using UnityEngine;

public class BeeIdleState : BeeState
{
    public BeeIdleState(BeeStateMachine stateMachine, BeeAI bee) : base(stateMachine, bee) { }
    

    private float waitTime = 0;
    [SerializeField, Range(0.1f,10)]private float minIdleTime = 2;
    [SerializeField, Range(0.1f, 10)] private float maxIdleTime = 6;
    public override void EnterState() {
        //-------------------
        //      Set state enum
        //-------------------
        bee.state = BeeStates.Idle;

        //-------------------
        //      Timer setup
        //-------------------
        waitTime = Random.Range(minIdleTime, maxIdleTime);
        waitTime += Time.time;
        
        //Debug.Log("Bee is in Idle State, Wait time is:" +waitTime);
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate()
    {
        if (waitTime >= Time.time) return;
        stateMachine.ChangeState(bee.moveingState);        
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
