public abstract class PlayerState 
{
    protected PlayerStateMachine stateMachine;
    protected PlayerMovemant player;

    public PlayerState(PlayerStateMachine stateMachine, PlayerMovemant player)
    {
        this.stateMachine = stateMachine;
        this.player = player;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(PlayerMovemant.AnimationTriggers triggerType) { }


}
