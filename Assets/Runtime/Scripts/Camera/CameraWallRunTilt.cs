using Cinemachine; // Use 'using Cinemachine;' if you are on an older version
using UnityEngine;

namespace NetworkBaseRuntime
{
    public class CameraWallRunTilt : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera _vCam;
        private WallCheck _wallCheck;

        public void SetWallCheck(WallCheck wallCheck) => _wallCheck = wallCheck;

        [Header("Tilt Settings")]
        [SerializeField] private float _tiltAngle = 15f; // How extreme the tilt is
        [SerializeField] private float _tiltSpeed = 3f;  // How fast it snaps to the tilt

        private void Reset()
        {
            _vCam = GetComponent<CinemachineVirtualCamera>();
        }

        private void Update()
        {
            if (_vCam == null || _wallCheck == null) return;

            float targetDutch = 0f;

            // If wall is on the left, tilt right (positive Dutch). 
            // If wall is on the right, tilt left (negative Dutch).
            if (_wallCheck.IsWallLeft)
            {
                targetDutch = -_tiltAngle;
            }
            else if (_wallCheck.IsWallRight)
            {
                targetDutch = _tiltAngle;
            }

            // Smoothly interpolate the camera's Dutch property
            _vCam.m_Lens.Dutch = Mathf.Lerp(_vCam.m_Lens.Dutch, targetDutch, Time.deltaTime * _tiltSpeed);
        }
    }
}