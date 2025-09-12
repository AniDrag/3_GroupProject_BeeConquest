using UnityEngine;

public class BeeCollectingPolinState : BeeStates
{
    public BeeCollectingPolinState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }
    private float nextCollectTime;
    public override void EnterState() {
        bee.beeState = BeeAI.BeeState.Collecting;
        nextCollectTime = Time.time + bee.collectionSpeed;
        Debug.Log("Collected polin");
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {
        if (nextCollectTime >= Time.time) return;
            bee.stateMachine.ChangeState(bee.moveingState);
            //bee.GetDestinationData();
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() {
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
