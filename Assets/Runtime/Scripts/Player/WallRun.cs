using UnityEngine;

namespace NetworkBaseRuntime
{
    public class WallRun : MonoBehaviour
    {
        private WallCheck _wallCheck;
        private GroundCheck _groundCheck;
        private PlayerStats _stats;

        public bool IsWallRunning { get; private set; }

        public void Init(PlayerStats stats, GroundCheck groundCheck, WallCheck wallCheck)
        {
            _stats = stats;
            _groundCheck = groundCheck;
            _wallCheck = wallCheck;
        }

        public Vector3 HandleWallRun(Vector3 frameVelocity, Vector2 input)
        {
            // 1. Exit Conditions
            // We check input.y to ensure they are holding "Forward" to maintain the run
            if (_groundCheck.IsGrounded || !_wallCheck.IsWall || input.y <= 0)
            {
                IsWallRunning = false;
                return frameVelocity;
            }

            IsWallRunning = true;

            // 2. Calculate Direction along the wall
            // Uses the WallNormal from WallCheck to find the parallel vector
            Vector3 wallForward = Vector3.Cross(_wallCheck.WallNormal, Vector3.up);

            // Align wallForward with the direction the player is actually facing
            if (Vector3.Dot(wallForward, transform.forward) < 0)
                wallForward = -wallForward;

            // 3. Horizontal Velocity
            // We use the specific WallRunSpeed from stats
            Vector3 horizontalVel = wallForward * _stats.WallRunSpeed;

            // 4. Vertical Velocity (The "Anti-Gravity" Feel)
            float verticalVel = frameVelocity.y;

            // If we just started wall running or are falling too fast, 
            // we snap the vertical velocity to a manageable "slide" speed
            if (verticalVel < -2f)
                verticalVel = -2f;

            // Apply the lighter wall gravity from stats
            verticalVel += _stats.WallRunGravity * Time.fixedDeltaTime;

            // 5. Stick Velocity
            // Pushes the player slightly INTO the wall so the SphereCast doesn't lose contact
            Vector3 stickVel = -_wallCheck.WallNormal * _stats.WallStickForce;

            // Return the combined vector
            return new Vector3(horizontalVel.x, verticalVel, horizontalVel.z) + stickVel;
        }
    }
}