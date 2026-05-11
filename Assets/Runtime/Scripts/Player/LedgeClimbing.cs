using UnityEngine;

namespace NetworkBaseRuntime
{
    public class LedgeClimbing : MonoBehaviour
    {
        private LedgeCheck _ledgeCheck;
        private GroundCheck _groundCheck;
        private PlayerStats _stats;

        public bool IsClimbing { get; private set; }


        public void _init(PlayerStats stats, GroundCheck groundCheck, LedgeCheck ledgeCheck)
        {
            _stats = stats;
            _groundCheck = groundCheck;
            _ledgeCheck = ledgeCheck;
        }
        public Vector3 HandleLedgeClimb(Vector3 frameVelocity, Vector2 input)
        {
            if (_groundCheck.IsGrounded || !_ledgeCheck.IsLedge || input.y <= 0)
            {
                IsClimbing = false;
                return frameVelocity;
            }
            IsClimbing = true;
            // We will snap the player to the ledge and then apply an upwards velocity to simulate climbing
            Vector3 climbVelocity = new Vector3(0, _stats.LedgeClimbSpeed, 0);
            return climbVelocity;
        }
    }
}
