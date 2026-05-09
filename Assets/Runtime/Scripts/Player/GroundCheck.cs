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

        public void Init(PlayerStats stats)
        {
            _col = GetComponent<CapsuleCollider>();
            _stats = stats;
        }

        public void Check(float time)
        {
            // Build capsule points for cast
            Vector3 center = _col.bounds.center;
            float halfHeight = _col.height / 2f - _col.radius;
            Vector3 p1 = center + Vector3.up * halfHeight;
            Vector3 p2 = center - Vector3.up * halfHeight;
            int mask = ~_stats.PlayerLayer;

            bool groundHit = Physics.CapsuleCast(feet.position, p2, _col.radius, Vector3.down, _stats.GrounderDistance, mask);
            bool ceilingHit = Physics.CapsuleCast(feet.position, p2, _col.radius, Vector3.up, _stats.GrounderDistance, mask);

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
            if (_col == null) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_col.bounds.center + Vector3.down * (_col.height / 2f), _col.radius);
        }
    }
}