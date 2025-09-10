using TMPro;
using UnityEngine;

public class BeeMoveToTargetState : BeeState
{
    public BeeMoveToTargetState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }
    public override void EnterState()
    {
        bee.state = BeeStates.Moving;
        //Debug.Log("Bee is in normal moving State");
        
    }
    public override void ExitState() { }
    public override void LogicUpdate() 
    {
        if (bee.targetPosition != null && !bee.atDestination)
        {
            //.Log("moving Bee");
            bee.transform.position = Vector3.MoveTowards(bee.transform.position, bee.targetPosition, bee.speed * Time.deltaTime);
        }
    }
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
        else if (bee.getingPollin)
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
