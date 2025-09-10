using UnityEngine;

public class BeeChasePlayerState : BeeState
{
    public BeeChasePlayerState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }

    public override void EnterState() {
        //Debug.Log("Bee is in chase player moving State");
        bee.state = BeeStates.Moving;
        bee.PING_CatchPlayer();
        //stateMachine.ChangeState(bee.moveingState);
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() {

        if (bee.player != null && !bee.atDestination)
        {
            //.Log("moving Bee");
            bee.transform.position = Vector3.MoveTowards(bee.transform.position, bee.targetPosition, bee.speed * Time.deltaTime);
        }
        else
        {
            stateMachine.ChangeState(bee.idleState);
        }
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
