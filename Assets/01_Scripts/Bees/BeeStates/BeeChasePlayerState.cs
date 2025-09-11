using UnityEngine;

public class BeeChasePlayerState : BeeStates
{
    public BeeChasePlayerState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }

    public override void EnterState() {
        Debug.Log("Bee is in chase player moving State");
        bee.beeState = BeeAI.BeeState.Following;
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() 
    {
        if (!bee.playerComand && bee.atDestination)// playe comand prevents it from moving to another state and always follow
        {
            stateMachine.ChangeState(bee.idleState);
            return;
        }

        bee.SetDestination(bee.TargetFollow.position);
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
