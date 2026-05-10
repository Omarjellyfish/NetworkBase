using System;
using UnityEngine;

namespace NetworkBaseRuntime
{
    public class JumpController : MonoBehaviour
    {
        public event Action Jumped;

        private PlayerStats _stats;
        private GroundCheck _groundCheck;
        private WallCheck _wallCheck; // New dependency

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump(float time) =>
            _bufferedJumpUsable && time < _timeJumpWasPressed + _stats.JumpBuffer;

        private bool CanUseCoyote(float time) =>
            _coyoteUsable && !_groundCheck.IsGrounded &&
            time < _groundCheck.TimeLeftGrounded + _stats.CoyoteTime;

        // Wall jump doesn't usually use coyote time, but checks if a wall is present
        private bool CanWallJump() => !_groundCheck.IsGrounded && _wallCheck.IsWall;

        public void Init(PlayerStats stats, GroundCheck groundCheck, WallCheck wallCheck)
        {
            _stats = stats;
            _groundCheck = groundCheck;
            _wallCheck = wallCheck;

            groundCheck.GroundedChanged += (grounded, _) =>
            {
                if (grounded)
                {
                    _coyoteUsable = true;
                    _bufferedJumpUsable = true;
                    _endedJumpEarly = false;
                }
            };
        }

        public void RequestJump(float time)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = time;
        }

        public Vector3 HandleJump(Vector3 frameVelocity, bool jumpHeld, float time)
        {
            // 1. Handle Jump Termination (Variable Jump Height)
            if (!_endedJumpEarly && !_groundCheck.IsGrounded && !jumpHeld && frameVelocity.y > 0)
                _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump(time))
                return frameVelocity;

            // 2. Execute Ground Jump or Coyote Jump
            if (_groundCheck.IsGrounded || CanUseCoyote(time))
            {
                frameVelocity = ExecuteJump(frameVelocity);
            }
            // 3. Execute Wall Jump
            else if (CanWallJump())
            {
                frameVelocity = ExecuteWallJump(frameVelocity);
            }

            _jumpToConsume = false;
            return frameVelocity;
        }

        private Vector3 ExecuteJump(Vector3 frameVelocity)
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;

            frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
            return frameVelocity;
        }

        private Vector3 ExecuteWallJump(Vector3 frameVelocity)
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;

            // Apply upward force
            frameVelocity.y = _stats.WallJumpVerticalForce;

            // Apply horizontal kick away from the wall normal
            // We use the normal from WallCheck to push the player OUT
            Vector3 wallNormal = _wallCheck.WallNormal;
            frameVelocity.x = wallNormal.x * _stats.WallJumpForce;
            frameVelocity.z = wallNormal.z * _stats.WallJumpForce;

            Jumped?.Invoke();
            return frameVelocity;
        }

        public bool EndedJumpEarly => _endedJumpEarly;
    }
}