using UnityEngine;

public class BeeCollectingPolinState : BeeStates
{
    public BeeCollectingPolinState(BeeStateMachine StateMachine, BeeAI Bee) : base(StateMachine, Bee) { }
    private float waitTime = 0;
    [SerializeField, Range(0.1f, 10)] private float minIdleTime = 2;
    [SerializeField, Range(0.1f, 10)] private float maxIdleTime = 6;
    public override void EnterState() {
        bee.beeState = BeeAI.BeeState.Collecting;
        Debug.Log("Collected polin");
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {
       
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() {
        if (waitTime >= Time.time) return;
        stateMachine.ChangeState(bee.idleState);
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
