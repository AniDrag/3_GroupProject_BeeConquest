using UnityEngine;

public class BeeChasePlayerState : BeeStates
{
    public BeeChasePlayerState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }

    public override void EnterState() {
        //Debug.Log("Bee is in chase player moving State");
        bee.beeState = BeeAI.BeeState.Following;
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() 
    {
        if (!bee.playerComand && bee.atDestination)// playe comand prevents it from moving to another state and always follow
        {
            Debug.Log("Changing State BEE");
            stateMachine.ChangeState(bee.moveingState);
            return;
        }
        else
        {
            bee.SetDestination(bee.player.transform.position);
        }
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
