using UnityEngine;
using UnityEngine.InputSystem;

public class STATE_Crouching : PlayerState
{
    public STATE_Crouching(PlayerStateMachine stateMachine, PlayerMovemant player) : base(stateMachine, player) { }
    public string stateName = "Crouching";
    Vector3 moveDirection;
    Vector2 moveInput;
    public override void AnimationTriggerEvent(PlayerMovemant.AnimationTriggers triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log(stateName);
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        moveInput = player.input.actions["Move"].ReadValue<Vector2>();

       // if (player._inAir) idk but this should effect its entering state.
       // {
       //     stateMachine.ChangeState(player.fallingState);
       // }
        if (player.input.actions["Jump"].IsPressed() && player._isGrounded)
        {
            stateMachine.ChangeState(player.jumpingState);
        }
        else if (player.input.actions["Sprint"].IsPressed() && moveInput != Vector2.zero && player._isGrounded)
        {
            stateMachine.ChangeState(player.runningState);
        }
        else if (moveInput == Vector2.zero && player.input.actions["Crouch"].IsPressed())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
    public override void PhysicsUpdate()
    {
        moveDirection = player.GetMoveDirection(moveInput);
        player._body.AddForce(moveInput.normalized * player.crouchSpeed, ForceMode.Acceleration);
        player.ClampVelocity(player.crouchSpeed);
    }
}
