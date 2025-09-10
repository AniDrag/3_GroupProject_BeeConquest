using UnityEngine;

public class BeeIdleState : BeeState
{
    public BeeIdleState(BeeStateMachine stateMachine, BeeAI bee) : base(stateMachine, bee) { }
    

    private float waitTime = 0;
    [SerializeField, Range(0.1f,10)]private float minIdleTime = 2;
    [SerializeField, Range(0.1f, 10)] private float maxIdleTime = 6;
    public override void EnterState() {
        bee.state = BeeStates.Idle;
        waitTime = Random.Range(minIdleTime, maxIdleTime);
        waitTime += Time.time;
    }
    public override void ExitState() { }
    public override void LogicUpdate() { }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate()
    {
        if (waitTime < Time.time)
        {
            stateMachine.ChangeState(bee.moveingState);
        }
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
