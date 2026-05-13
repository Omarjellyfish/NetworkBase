using Cinemachine;
using UnityEngine;

namespace NetworkBaseRuntime
{
    /// <summary>
    /// Lives on the CameraSetup in the scene. When the local player spawns,
    /// sets Cinemachine Follow/LookAt targets and syncs rotation every frame
    /// (HardLockToTarget only handles position, not rotation).
    /// </summary>
    public class AttachCameraToPlayer : MonoBehaviour
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineStateDrivenCamera _stateDrivenCamera;

        [Header("Camera Effects")]
        [SerializeField] private CameraFovScaler _fovScaler;
        [SerializeField] private CameraWallRunTilt _wallRunTilt;

        private Transform _cameraRoot;

        private void OnEnable()
        {
            PlayerInputHandler.OnLocalPlayerSpawned += HandleLocalPlayerSpawned;
            PlayerInputHandler.OnLocalPlayerDespawned += HandleLocalPlayerDespawned;
        }

        private void OnDisable()
        {
            PlayerInputHandler.OnLocalPlayerSpawned -= HandleLocalPlayerSpawned;
            PlayerInputHandler.OnLocalPlayerDespawned -= HandleLocalPlayerDespawned;
        }

        private void HandleLocalPlayerSpawned(Transform cameraRoot)
        {
            if (cameraRoot == null)
            {
                Debug.LogError("[AttachCameraToPlayer] Camera root is null — cannot attach.");
                return;
            }

            _cameraRoot = cameraRoot;

            if (_stateDrivenCamera != null)
            {
                _stateDrivenCamera.Follow = cameraRoot;
                _stateDrivenCamera.LookAt = cameraRoot;
                Debug.Log($"[AttachCameraToPlayer] Cinemachine targets set to '{cameraRoot.name}'.");
            }

            // Wire up camera effect scripts that need player references
            Rigidbody playerRb = cameraRoot.GetComponentInParent<Rigidbody>();
            WallCheck wallCheck = cameraRoot.GetComponentInParent<WallCheck>();

            if (_fovScaler != null && playerRb != null)
            {
                _fovScaler.SetPlayerRigidbody(playerRb);
            }

            if (_wallRunTilt != null && wallCheck != null)
            {
                _wallRunTilt.SetWallCheck(wallCheck);
            }
        }

        private void LateUpdate()
        {
            // HardLockToTarget only syncs position — we must sync rotation manually
            if (_cameraRoot != null && _stateDrivenCamera != null)
            {
                _stateDrivenCamera.transform.SetPositionAndRotation(
                    _cameraRoot.position, _cameraRoot.rotation);
            }
        }

        private void HandleLocalPlayerDespawned()
        {
            _cameraRoot = null;

            if (_stateDrivenCamera != null)
            {
                _stateDrivenCamera.Follow = null;
                _stateDrivenCamera.LookAt = null;
            }

            if (_fovScaler != null)
            {
                _fovScaler.SetPlayerRigidbody(null);
            }

            if (_wallRunTilt != null)
            {
                _wallRunTilt.SetWallCheck(null);
            }

            Debug.Log("[AttachCameraToPlayer] Camera detached from player.");
        }
    }
}
