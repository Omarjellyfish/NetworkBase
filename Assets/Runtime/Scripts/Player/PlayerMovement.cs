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
            Vector3 targetHorizontal = new Vector3(input.x, 0, input.y) * _stats.MaxSpeed;
            Vector3 current = new Vector3(frameVelocity.x, 0, frameVelocity.z);

            float accel = (targetHorizontal == Vector3.zero)
                ? (_groundCheck.IsGrounded ? _stats.GroundDeceleration : _stats.AirDeceleration)
                : _stats.Acceleration;

            Vector3 result = Vector3.MoveTowards(current, targetHorizontal, accel * Time.fixedDeltaTime);

            return new Vector3(result.x, frameVelocity.y, result.z);
        }
    }
}