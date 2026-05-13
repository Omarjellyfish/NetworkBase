using UnityEngine;
using Cinemachine;
    namespace NetworkBaseRuntime
{
    public class CameraFovScaler : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _vCam;
        [SerializeField] private PlayerStats _stats;
        private Rigidbody _playerRb;

        public void SetPlayerRigidbody(Rigidbody rb) => _playerRb = rb;

        [Header("FOV Settings")]
        [SerializeField] private float _minFov = 60f;
        [SerializeField] private float _maxFov = 90f;

        private void Reset()
        {
            // Auto-assign the camera if attached to the same object
            _vCam = GetComponent<CinemachineVirtualCamera>();
        }

        private void Update()
        {
            if (_playerRb == null || _vCam == null || _stats == null) return;

            // Get flat horizontal speed (ignoring falling speed)
            float currentSpeed = new Vector3(_playerRb.linearVelocity.x, 0, _playerRb.linearVelocity.z).magnitude;

            // Find the highest possible intended speed from your stats to use as our threshold
            float topSpeedThreshold = Mathf.Max(_stats.MaxSpeed, _stats.WallRunSpeed);

            // Calculate how close we are to that top speed (0 to 1)
            float speedPercentage = Mathf.Clamp01(currentSpeed / topSpeedThreshold);

            // Map that percentage to our FOV range
            float targetFov = Mathf.Lerp(_minFov, _maxFov, speedPercentage);

            // Smoothly transition the camera's FOV
            _vCam.m_Lens.FieldOfView = Mathf.Lerp(_vCam.m_Lens.FieldOfView, targetFov, Time.deltaTime * 5f);
        }
    }
}