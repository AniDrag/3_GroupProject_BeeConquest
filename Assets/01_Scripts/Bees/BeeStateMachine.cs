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
    protected PlayerMovemant player;

    public BeeState(BeeStateMachine StateMachine, PlayerMovemant player, BeeStates setState)
    {
        state = setState;
        this.stateMachine = StateMachine;
        this.player = player;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void LogicUpdate() { }
    public virtual void LateLogicUpdate() { }
    public virtual void FixedLogicUpdate() { }
    public virtual void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }
}
