using UnityEngine;
using UnityEngine.InputSystem;

public class STATE_Running : PlayerState
{
    public STATE_Running(PlayerStateMachine stateMachine, PlayerMovemant player) : base(stateMachine, player) { }// hmmm, HMMMMM, HAAAMMMMMM i should ad the input system here.. yes i should
    public string stateName = "Running";
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

        

        if (player._inAir)
        {
            stateMachine.ChangeState(player.fallingState);
        }
        else if (player.input.actions["Crouch"].IsPressed()) 
        {
            //if (player.IsSprinting) // add condition
            //    stateMachine.ChangeState(player.slidingState);
            //else
            stateMachine.ChangeState(player.crouchingState);
        }
        else if (player.input.actions["Jump"].IsPressed() && player._isGrounded)
        {
            stateMachine.ChangeState(player.jumpingState);
        }
        else if (!player.input.actions["Sprint"].IsPressed() && moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.walkingState);
        }
        else if (moveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
    public override void PhysicsUpdate()
    {
        moveDirection = player.GetMoveDirection(moveInput);
        player._body.AddForce(moveDirection * player.runSpeed, ForceMode.Acceleration);
        player.ClampVelocity(player.runSpeed);
    }
}
