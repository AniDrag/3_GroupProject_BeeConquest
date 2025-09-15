using UnityEngine;

public class BeeSpawnTokenState : BeeStates
{
    public BeeSpawnTokenState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }
    //Variabbles




    //----------------------
    public override void EnterState()
    {
        bee.beeState = BeeAI.BeeState.Collecting;
        
        //Debug.Log("Collected polin");
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {    
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() // has a tick speed
    {
        //bee.stateMachine.ChangeState(bee.moveingState); Call any state from bee
        //bee.GetDestinationData();
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
