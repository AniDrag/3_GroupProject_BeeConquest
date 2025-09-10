using UnityEngine;

public class BeeStateMachine : MonoBehaviour
{
    public BeeStates currentState { get; private set; }

    public void Initialize(BeeStates startingState)
    {
        currentState = startingState;
        currentState.EnterState();
    }

    public void ChangeState(BeeStates newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}

public class BeeStates
{
    public BeeAI.BeeState state;
    protected BeeStateMachine stateMachine;
    protected BeeAI bee;

    public BeeStates(BeeStateMachine StateMachine, BeeAI bee)
    {
        this.stateMachine = StateMachine;
        this.bee = bee;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void LogicUpdate() { }
    public virtual void LateLogicUpdate() { }
    public virtual void FixedLogicUpdate() { }
    public virtual void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
