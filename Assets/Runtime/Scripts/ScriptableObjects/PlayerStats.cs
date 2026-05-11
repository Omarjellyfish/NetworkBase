using UnityEngine;

namespace NetworkBaseRuntime
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "NetworkBaseRuntime/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        public float MaxSpeed = 5f;
        public float Acceleration = 50f;
        public float GroundDeceleration = 50f;
        public float AirDeceleration = 20f;

        [Header("Jump")]
        public float JumpPower = 10f;
        public float CoyoteTime = 0.15f;
        public float JumpBuffer = 0.1f;
        public float JumpEndEarlyGravityModifier = 3f;

        [Header("Wall Run")]
        public float WallRunSpeed = 8f;           // Horizontal speed while on wall
        public float WallRunGravity = 2f;         // Downward pull while wall running
        public float WallStickForce = 5f;         // Force pushing player into the wall
        public float WallJumpForce = 10f;         // Horizontal "kick" away from wall
        public float WallJumpVerticalForce = 8f;  // Upward "kick" during wall jump
        public LayerMask WallLayer;               // What counts as a wall?


        [Header("Ledge Grab")]
        public float LedgeClimbSpeed = 10f;


        [Header("Gravity")]
        public float FallAcceleration = 50f;
        public float MaxFallSpeed = 20f;
        public float GroundingForce = -1.5f;

        [Header("Collision/Detection")]
        public float GrounderDistance = 0.1f;
        public float WallCheckDistance = 0.5f;    // Distance for the SphereCast
        public float WallCheckRadius = 0.3f;      // Thickness of the detection sphere
        public LayerMask PlayerLayer;             // The player's own layer (to ignore)

        [Header("Input")]
        public bool SnapInput = true;
        public float HorizontalDeadZoneThreshold = 0.1f;
        public float VerticalDeadZoneThreshold = 0.1f;

        [Header("Look / Camera")]
        public float MouseSensitivity = 25f;
        public float UpAndDownClamp = 85f; // Prevents looking past straight up/down
    }
}