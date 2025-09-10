using UnityEngine;

public class BeeCombatState : BeeState
{
    public BeeCombatState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }

    public override void EnterState() {
        bee.state = BeeStates.Combat;
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {

    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() { }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
