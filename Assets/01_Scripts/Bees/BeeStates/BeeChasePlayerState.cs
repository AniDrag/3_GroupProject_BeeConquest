using UnityEngine;

public class BeeChasePlayerState : BeeStates
{
    public BeeChasePlayerState(BeeStateMachine StateMachine, BasicBee Bee) : base(StateMachine, Bee) { }

    public override void EnterState() {
        //Debug.Log("Bee is in chase player moving State");
        bee.beeState = BeeState.Following;
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() 
    {
        Debug.Log($"Chasing State update, currently i will check the field, is it null? {bee.player.currentField == null} ");
        if (!bee.playerComand && bee.atDestination || bee.player.currentField != null)// playe comand prevents it from moving to another state and always follow
        {
            Debug.Log("Chasing State BEE, should be called after Chasing State update");
            stateMachine.ChangeState(bee.moveingState);
            return;
        }
        else if (!bee.player.isConvertingPollen)
        {
            bee.SetDestination(bee.player.transform.position);
        }
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
