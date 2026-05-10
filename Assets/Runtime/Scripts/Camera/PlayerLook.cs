using UnityEngine;

namespace NetworkBaseRuntime
{
    public class PlayerLook : MonoBehaviour
    {
        private PlayerStats _stats;
        private Transform _cameraRoot;

        private float _xRotation; // Tracks our up/down tilt

        public void Init(PlayerStats stats, Transform cameraRoot)
        {
            _stats = stats;
            _cameraRoot = cameraRoot;
        }

        public void HandleLook(Vector2 lookInput)
        {
            if (_cameraRoot == null) return;

            // 1. Calculate input based on sensitivity
            // Note: We don't use Time.deltaTime here because Mouse Delta already scales with framerate in the New Input System
            float mouseX = lookInput.x * _stats.MouseSensitivity * 0.01f;
            float mouseY = lookInput.y * _stats.MouseSensitivity * 0.01f;

            // 2. Calculate vertical rotation (Pitch)
            _xRotation -= mouseY; // Subtracting because moving mouse up gives a positive Y, but rotating UP in Unity is negative X

            // Clamp the rotation so we don't look behind ourselves
            _xRotation = Mathf.Clamp(_xRotation, -_stats.UpAndDownClamp, _stats.UpAndDownClamp);

            // 3. Apply vertical rotation to the Camera Root
            _cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

            // 4. Apply horizontal rotation (Yaw) to the whole Player body
            transform.Rotate(Vector3.up * mouseX);
        }
    }
}