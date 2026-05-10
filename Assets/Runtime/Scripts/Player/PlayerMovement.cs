using UnityEngine;

namespace NetworkBaseRuntime
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerStats _stats;
        private GroundCheck _groundCheck;

        public void Init(PlayerStats stats, GroundCheck groundCheck)
        {
            _stats = stats;
            _groundCheck = groundCheck;
        }

        public Vector3 HandleDirection(Vector3 frameVelocity, Vector2 input)
        {
            // 1. Calculate direction relative to where the player is looking
            Vector3 moveDirection = (transform.forward * input.y + transform.right * input.x);

            // Normalize it so moving diagonally isn't faster than moving straight
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            // Apply speed
            Vector3 targetHorizontal = moveDirection * _stats.MaxSpeed;

            // 2. Current horizontal velocity
            Vector3 current = new Vector3(frameVelocity.x, 0, frameVelocity.z);

            // 3. Determine acceleration/deceleration
            float accel = (targetHorizontal == Vector3.zero)
                ? (_groundCheck.IsGrounded ? _stats.GroundDeceleration : _stats.AirDeceleration)
                : _stats.Acceleration;

            // 4. Smoothly move towards the target speed
            Vector3 result = Vector3.MoveTowards(current, targetHorizontal, accel * Time.fixedDeltaTime);

            return new Vector3(result.x, frameVelocity.y, result.z);
        }
    }
}