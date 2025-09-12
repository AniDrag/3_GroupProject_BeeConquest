using UnityEngine;

public class BeeCombatState : BeeStates
{
    public BeeCombatState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }

    private float attackInterval = 1f; // seconds per attack
    private float nextAttackTime;

    public override void EnterState() {
        bee.beeState = BeeAI.BeeState.Attacking;
        nextAttackTime = Time.time; // can attack immediately
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {
       
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate()
    {
        if (bee.TargetEnemy == null)
        {
            bee.stateMachine.ChangeState(bee.idleState);
            return;
        }

        if (!bee.atDestination)
        {
            bee.SetDestination(bee.TargetEnemy.transform.position);
            return;
        }

        // Attack only when cooldown has passed
        if (Time.time < nextAttackTime) return;

        bee.TargetEnemy.TakeDamage(bee.damage);
        nextAttackTime = Time.time + attackInterval; // reset cooldown
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
