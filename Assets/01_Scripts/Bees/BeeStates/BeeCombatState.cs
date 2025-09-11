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
    public override void FixedLogicUpdate()
    {
        if ( bee.TargetEnemy == null) stateMachine.ChangeState(bee.idleState);

        else if (bee.atDestination) bee.TargetEnemy.TakeDamage(bee.damage);

        else bee.SetDestination(bee.TargetEnemy.transform.position);
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
