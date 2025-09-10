using UnityEngine;

public class BeeCombatState : BeeStates
{
    public BeeCombatState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }

    public override void EnterState() {
        bee.beeState = BeeAI.BeeState.Attacking;
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {

    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() { }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
