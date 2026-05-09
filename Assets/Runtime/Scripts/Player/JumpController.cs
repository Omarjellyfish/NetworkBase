using System;
using UnityEngine;

namespace NetworkBaseRuntime
{
    public class JumpController : MonoBehaviour
    {
        public event Action Jumped;

        private PlayerStats _stats;
        private GroundCheck _groundCheck;

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

        public void Init(PlayerStats stats, GroundCheck groundCheck)
        {
            _stats = stats;
            _groundCheck = groundCheck;

            // Reset buffer when landing
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
            // Shorten jump if button released early
            if (!_endedJumpEarly && !_groundCheck.IsGrounded && !jumpHeld && frameVelocity.y > 0)
                _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump(time))
                return frameVelocity;

            if (_groundCheck.IsGrounded || CanUseCoyote(time))
                frameVelocity = ExecuteJump(frameVelocity);

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

        public bool EndedJumpEarly => _endedJumpEarly;
    }
}