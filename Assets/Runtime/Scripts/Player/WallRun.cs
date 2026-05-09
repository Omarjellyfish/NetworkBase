using UnityEngine;

namespace NetworkBaseRuntime
{
    [RequireComponent(typeof(Rigidbody))]
    public class WallRun : MonoBehaviour
    {
        [SerializeField] private float wallRunGravity = 2f;
        [SerializeField] private float wallRunSpeed = 6f;

        private Rigidbody _rb;
        private GroundCheck _groundCheck;
        private WallCheck _wallCheck;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _groundCheck = GetComponent<GroundCheck>();
            _wallCheck = GetComponent<WallCheck>();
        }

        public void HandleWallRun()
        {
            if (_groundCheck.IsGrounded || !_wallCheck.IsWall) return;

            // Counteract gravity so the player sticks to the wall
            _rb.AddForce(Vector3.up * wallRunGravity, ForceMode.Acceleration);

            // Push player along the wall surface (forward relative to the wall)
            Vector3 wallForward = Vector3.Cross(_wallCheck.WallNormal, Vector3.up);

            // Flip direction if it's pointing backwards relative to the player
            if (Vector3.Dot(wallForward, transform.forward) < 0)
                wallForward = -wallForward;

            _rb.AddForce(wallForward * wallRunSpeed, ForceMode.Acceleration);

            // Push player into the wall to maintain contact
            _rb.AddForce(-_wallCheck.WallNormal * 10f, ForceMode.Acceleration);
        }
    }
}