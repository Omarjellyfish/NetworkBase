using UnityEngine;

namespace NetworkBaseRuntime
{
    public class WallCheck : MonoBehaviour
    {
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private float sphereRadius = 0.3f;
        [SerializeField] private LayerMask wallLayer;

        public bool IsWall => IsWallLeft || IsWallRight;
        public bool IsWallLeft { get; private set; }
        public bool IsWallRight { get; private set; }

        public RaycastHit LeftHit { get; private set; }
        public RaycastHit RightHit { get; private set; }

        public Vector3 WallNormal => IsWallRight ? RightHit.normal : (IsWallLeft ? LeftHit.normal : Vector3.zero);

        public void Check()
        {
            // Right Check
            IsWallRight = Physics.SphereCast(transform.position, sphereRadius,
                transform.right, out RaycastHit rightHit, wallCheckDistance, wallLayer);
            RightHit = rightHit;

            // Left Check
            IsWallLeft = Physics.SphereCast(transform.position, sphereRadius,
                -transform.right, out RaycastHit leftHit, wallCheckDistance, wallLayer);
            LeftHit = leftHit;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsWallRight ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.right * wallCheckDistance, sphereRadius);

            Gizmos.color = IsWallLeft ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position - transform.right * wallCheckDistance, sphereRadius);
        }
    }
}