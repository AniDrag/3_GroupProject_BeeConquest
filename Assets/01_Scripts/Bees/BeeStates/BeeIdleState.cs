using UnityEngine;

public class BeeIdleState : BeeStates
{
    public BeeIdleState(BeeStateMachine stateMachine, BasicBee bee) : base(stateMachine, bee) { }


    private float waitEndTime;
    private float minIdleTime = 3f;
    private float maxIdleTime = 8f;
    public override void EnterState() {
        
        bee.beeState = BeeState.Idle;
        stateMachine.animator.SetTrigger(bee.beeState.ToString());  
        float waitDuration = Random.Range(minIdleTime, maxIdleTime);
        waitEndTime = Time.time + waitDuration;
        //Debug.Log("Bee is in Idle State, Wait time is:" +waitTime);
    }
    public override void ExitState() { }
    public override void LogicUpdate() {
        if (bee.playerComand) return;

        if (waitEndTime >= Time.time && bee.player.currentField == null) return;
        // bee.player.currentField == null Added so tha if we are on feil bees wount wait around
        stateMachine.ChangeState(bee.moveingState);
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate(){
        
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
