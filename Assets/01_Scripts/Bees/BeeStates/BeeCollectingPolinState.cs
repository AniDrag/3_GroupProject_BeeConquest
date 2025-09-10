using UnityEngine;

public class BeeCollectingPolinState : BeeStates
{
    public BeeCollectingPolinState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }
    
    public override void EnterState() {
        bee.beeState = BeeAI.BeeState.Collecting;
        Debug.Log("Collected polin");
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {
        stateMachine.ChangeState(bee.idleState);
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() { }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
