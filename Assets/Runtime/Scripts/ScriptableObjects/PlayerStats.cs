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

        [Header("Gravity")]
        public float FallAcceleration = 50f;
        public float MaxFallSpeed = 20f;
        public float GroundingForce = -1.5f;

        [Header("Collision")]
        public float GrounderDistance = 0.1f;
        public LayerMask PlayerLayer;

        [Header("Input")]
        public bool SnapInput = true;
        public float HorizontalDeadZoneThreshold = 0.1f;
        public float VerticalDeadZoneThreshold = 0.1f;
    }
}