using UnityEngine;
using UnityEngine.InputSystem;

public class STATE_Idle : PlayerState
{
    public STATE_Idle(PlayerStateMachine stateMachine, PlayerMovemant player) : base(stateMachine, player) { }
    public string stateName = "Idle";
    Vector2 moveInput;
    public Animation animation;
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

        if (player.input.actions["Crouch"].IsPressed()) // any state can crouch
        {
            stateMachine.ChangeState(player.crouchingState);
        }

        if (player._inAir)
        {
            stateMachine.ChangeState(player.fallingState);
        }
        else if (player.input.actions["Jump"].IsPressed() && player._isGrounded)
        {
            stateMachine.ChangeState(player.jumpingState);
        }
        else if (player.input.actions["Sprint"].IsPressed() && moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.runningState);
        }
        else if (moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.walkingState);
        }
    }

    public override void PhysicsUpdate()
    {
        //if (player._body.linearVelocity.y <= 0)
        //    stateMachine.ChangeState(player.fallingState);
        if(player._isGrounded && moveInput.magnitude < 0.01f)
          player._body.linearVelocity = Vector3.Lerp(player._body.linearVelocity, Vector3.zero, player._deceleration); // OnSlope()?10:1* should be If sliding then lerp is slower

    }
}

