using UnityEngine;

public class BeeCollectingPolinState : BeeState
{
    public BeeCollectingPolinState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }

    public override void EnterState() { }
    public override void ExitState() { }
    public override void LogicUpdate()
    {

    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() { }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
