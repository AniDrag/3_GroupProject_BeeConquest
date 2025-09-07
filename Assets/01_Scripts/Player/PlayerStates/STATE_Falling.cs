using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class STATE_Falling : PlayerState
{
    public STATE_Falling(PlayerStateMachine stateMachine, PlayerMovemant player) : base(stateMachine, player) { }
    public string stateName = "Falling";
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

        if (player._isGrounded && moveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if (player.input.actions["Crouch"].IsPressed())// ig i should have faling crouching but can be managed with bools _inAir and stuff
        {
            //if (player.IsSprinting) // add condition
            //    stateMachine.ChangeState(player.slidingState);
            //else
            stateMachine.ChangeState(player.crouchingState);
        }
    }

    public override void PhysicsUpdate()
    {
        moveDirection = player.GetMoveDirection(moveInput);
        player._body.AddForce(moveDirection * player.walkSpeed, ForceMode.Acceleration);
        if (player._isGrounded && moveInput.magnitude < 0.01f)
        {
            Vector3 vel = player._body.linearVelocity;

            // Only decelerate XZ, keep Y untouched
            Vector3 horizontal = new Vector3(vel.x, 0f, vel.z);
            horizontal = Vector3.Lerp(horizontal, Vector3.zero, player._deceleration * 2);

            player._body.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
        }
    }
}
