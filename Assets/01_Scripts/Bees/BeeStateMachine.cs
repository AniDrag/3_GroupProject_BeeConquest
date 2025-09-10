using UnityEngine;

public class BeeStateMachine : MonoBehaviour
{
    public BeeState currentState { get; private set; }

    public void Initialize(BeeState startingState)
    {
        currentState = startingState;
        currentState.EnterState();
    }

    public void ChangeState(BeeState newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}

public class BeeState
{
    public BeeStates state;
    protected BeeStateMachine stateMachine;
    protected BeeAI bee;

    public BeeState(BeeStateMachine StateMachine, BeeAI bee)
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
