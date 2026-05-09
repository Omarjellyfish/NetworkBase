using UnityEngine;

namespace NetworkBaseRuntime
{
    public class GravityController : MonoBehaviour
    {
        private PlayerStats _stats;
        private GroundCheck _groundCheck;
        private JumpController _jump;

        public void Init(PlayerStats stats, GroundCheck groundCheck, JumpController jump)
        {
            _stats = stats;
            _groundCheck = groundCheck;
            _jump = jump;
        }

        public Vector3 HandleGravity(Vector3 frameVelocity)
        {
            if (_groundCheck.HitCeiling && frameVelocity.y > 0)
                frameVelocity.y = 0;

            if (_groundCheck.IsGrounded && frameVelocity.y <= 0f)
            {
                frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                float gravity = _stats.FallAcceleration;
                if (_jump.EndedJumpEarly && frameVelocity.y > 0)
                    gravity *= _stats.JumpEndEarlyGravityModifier;

                frameVelocity.y = Mathf.MoveTowards(
                    frameVelocity.y, -_stats.MaxFallSpeed, gravity * Time.fixedDeltaTime);
            }

            return frameVelocity;
        }
    }
}