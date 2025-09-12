using UnityEngine;

public class BeeIdleState : BeeStates
{
    public BeeIdleState(BeeStateMachine stateMachine, BeeAI bee) : base(stateMachine, bee) { }


    private float waitEndTime;
    private float minIdleTime = 1f;
    private float maxIdleTime = 3f;
    public override void EnterState() {

        bee.beeState = BeeAI.BeeState.Idle;
        float waitDuration = Random.Range(minIdleTime, maxIdleTime);
        waitEndTime = Time.time + waitDuration;

        //Debug.Log("Bee is in Idle State, Wait time is:" +waitTime);
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate(){
        if (waitEndTime >= Time.time) return;
        stateMachine.ChangeState(bee.moveingState);
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
