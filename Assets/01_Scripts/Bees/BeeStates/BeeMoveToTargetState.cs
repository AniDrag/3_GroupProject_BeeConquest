//using UnityEngine;

public class BeeMoveToTargetState : BeeStates
{
    public BeeMoveToTargetState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }
    public override void EnterState()
    {
        bee.beeState = BeeAI.BeeState.Moving;
        
        bee.GetDestinationData();
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate()
    {  
        if (!bee.atDestination) return;
        //Debug.Log("Bee is at destination");
        // Decide what to do next
        if (bee.TargetEnemy != null)
        {
            //Debug.Log("attacking enemy");
            stateMachine.ChangeState(bee.combatState);
        }
        else if (bee.player.currentField != null)
        {
            //Debug.Log("getting pollin");
            stateMachine.ChangeState(bee.pollinCollectionState);
        }
        else
        {
            //Debug.Log("resting");          
            stateMachine.ChangeState(bee.idleState);
        }
        
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
