using UnityEngine;

namespace NetworkBaseRuntime
{
    public class WallCheck : MonoBehaviour
    {
        [SerializeField] private float wallCheckDistance = 0.7f;
        [SerializeField] private LayerMask wallLayer;

        public bool IsWall => IsWallLeft || IsWallRight;
        public bool IsWallLeft { get; private set; }
        public bool IsWallRight { get; private set; }

        public RaycastHit LeftHit { get; private set; }
        public RaycastHit RightHit { get; private set; }

        /// <summary>
        /// Returns the normal of whichever wall is detected (-1 = left, 1 = right, 0 = none)
        /// </summary>
        public Vector3 WallNormal
        {
            get
            {
                if (IsWallRight) return RightHit.normal;
                if (IsWallLeft) return LeftHit.normal;
                return Vector3.zero;
            }
        }

        public void Check()
        {
            IsWallRight = Physics.SphereCast(transform.position, wallCheckDistance,
                transform.right, out RaycastHit rightHit, wallCheckDistance, wallLayer);

            IsWallLeft = Physics.SphereCast(transform.position, wallCheckDistance,
                -transform.right, out RaycastHit leftHit, wallCheckDistance, wallLayer);

            RightHit = rightHit;
            LeftHit = leftHit;

            if (IsWallRight) Debug.Log($"Wall on RIGHT: {RightHit.collider.name}");
            if (IsWallLeft) Debug.Log($"Wall on LEFT: {LeftHit.collider.name}");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsWallRight ? Color.green : Color.blue;
            Gizmos.DrawRay(transform.position, transform.right * wallCheckDistance);

            Gizmos.color = IsWallLeft ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, -transform.right * wallCheckDistance);
        }
    }
}