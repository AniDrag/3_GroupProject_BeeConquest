using UnityEngine;

public class BeeIdleState : BeeState
{
    public BeeIdleState(BeeStateMachine StateMachine, PlayerMovemant player, BeeStates setState) : base(StateMachine, player, setState) { }

    public override void EnterState() { }
    public override void ExitState() { }
    public override void LogicUpdate() {
        
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() { }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
