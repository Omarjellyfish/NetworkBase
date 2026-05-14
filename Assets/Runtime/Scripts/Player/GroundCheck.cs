using System;
using UnityEngine;

namespace NetworkBaseRuntime
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class GroundCheck : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }
        public bool HitCeiling { get; private set; }
        public float TimeLeftGrounded { get; private set; }

        public event Action<bool, float> GroundedChanged;

        private CapsuleCollider _col;
        private PlayerStats _stats;
        [SerializeField] Transform feet;
        [SerializeField] float debugSphereRadius = 0.1f;
        public void Init(PlayerStats stats)
        {
            _col = GetComponent<CapsuleCollider>();
            _stats = stats;
        }

        public void Check(float time)
        {
            int mask = ~_stats.PlayerLayer;

            // Small, accurate SphereCast at the feet
            // Start slightly above the feet to prevent casting from inside the floor
            Vector3 groundOrigin = feet.position + (Vector3.up * debugSphereRadius);
            bool groundHit = Physics.SphereCast(groundOrigin, debugSphereRadius, Vector3.down, out _, debugSphereRadius + _stats.GrounderDistance, mask);

            // Small, accurate SphereCast at the head
            Vector3 headOrigin = _col.bounds.center + Vector3.up * (_col.height / 2f - debugSphereRadius);
            bool ceilingHit = Physics.SphereCast(headOrigin, debugSphereRadius, Vector3.up, out _, debugSphereRadius + _stats.GrounderDistance, mask);

            HitCeiling = ceilingHit;

            if (!IsGrounded && groundHit)
            {
                IsGrounded = true;
                GroundedChanged?.Invoke(true, 0);
            }
            else if (IsGrounded && !groundHit)
            {
                IsGrounded = false;
                TimeLeftGrounded = time;
                GroundedChanged?.Invoke(false, 0);
            }
        }

        private void OnDrawGizmos()
        {
            if (feet == null || _col == null) return;
            
            // Ground cast gizmo
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            
            Vector3 groundOrigin = feet.position + (Vector3.up * debugSphereRadius);
            float castDistance = _stats != null ? _stats.GrounderDistance : 0.1f;
            Vector3 groundEnd = groundOrigin + Vector3.down * (debugSphereRadius + castDistance);
            
            Gizmos.DrawWireSphere(groundOrigin, debugSphereRadius);
            Gizmos.DrawWireSphere(groundEnd, debugSphereRadius);
        }
    }
}