using UnityEngine;
using UnityEngine.InputSystem;

public class STATE_Jumping : PlayerState
{
    public STATE_Jumping(PlayerStateMachine stateMachine, PlayerMovemant player) : base(stateMachine, player) { }
    public string stateName = "Jumping";
    Vector3 moveDirection;
    Vector2 moveInput;
    public override void AnimationTriggerEvent(PlayerMovemant.AnimationTriggers triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        player._body.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
        Debug.Log(stateName);
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        moveInput = player.input.actions["Move"].ReadValue<Vector2>();

        if (player._isGrounded && moveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if (player.input.actions["Crouch"].IsPressed())// ig i should have faling crouching but can be managed with bools _inAir and stuff
        {
            stateMachine.ChangeState(player.crouchingState);
        }
    }

    public override void PhysicsUpdate()
    {

        moveDirection = player.GetMoveDirection(moveInput);
        player._body.AddForce(moveDirection * player.walkSpeed, ForceMode.Acceleration);

        if (player._body.linearVelocity.y < 0 && player._inAir)
        {
            stateMachine.ChangeState(new STATE_Falling(stateMachine, player));
        }
    }
}
