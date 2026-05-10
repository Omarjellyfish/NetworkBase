using UnityEngine;

namespace NetworkBaseRuntime
{
    public class SpeedEffectController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Material _effectMaterial;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PlayerStats _stats;

        [Header("Intensity Settings")]
        [SerializeField] private float _lerpSpeed = 1f;
        [Range(0, 1)][SerializeField] private float _minVisibleSpeedPercent = 0.5f;

        [Header("Alpha Settings")]
        [Tooltip("Speed percentage below which Alpha stays at 1")]
        [Range(0, 1)][SerializeField] private float _alphaThresholdPercent = 0.2f;
        [Tooltip("The lowest alpha value reached when at max speed")]
        [Range(0, 1)][SerializeField] private float _minAlphaValue = 0.5f;

        [Header("Mask Size Fine Tuning")]
        [Tooltip("X = Speed (0-1), Y = Mask Size Value")]
        [SerializeField] private AnimationCurve _maskSizeCurve = AnimationCurve.Linear(0, 1, 1, 0.5f);

        // Property IDs
        private static readonly int SpeedProp = Shader.PropertyToID("_Speed");
        private static readonly int AlphaProp = Shader.PropertyToID("_Alpha");
        private static readonly int MaskSizeProp = Shader.PropertyToID("_Mask_Size");

        private float _currentIntensity;
        private float _currentAlpha = 1f; // Tracked separately for smooth lerping

        private void Update()
        {
            if (_effectMaterial == null || _rb == null || _stats == null) return;

            // 1. Calculate horizontal speed
            float horizontalSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;

            // 2. Normalize intensity (0 to 1) for _Speed and Mask Size
            float targetIntensity = Mathf.InverseLerp(_stats.MaxSpeed * _minVisibleSpeedPercent, _stats.MaxSpeed, horizontalSpeed);
            _currentIntensity = Mathf.Lerp(_currentIntensity, targetIntensity, Time.deltaTime * _lerpSpeed);

            // 3. Calculate Alpha (1 below threshold, decreasing to min alpha towards max speed)
            float alphaThresholdSpeed = _stats.MaxSpeed * _alphaThresholdPercent;
            float targetAlpha = 1f; // Default to full opacity

            if (horizontalSpeed > alphaThresholdSpeed)
            {
                // Calculate progress from the threshold (0) up to MaxSpeed (1)
                float alphaProgress = Mathf.InverseLerp(alphaThresholdSpeed, _stats.MaxSpeed, horizontalSpeed);

                // Map that 0-1 progress to our 1.0 -> 0.5 alpha range
                targetAlpha = Mathf.Lerp(1f, _minAlphaValue, alphaProgress);
            }

            // Smoothly lerp the current alpha toward the target
            _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, Time.deltaTime * _lerpSpeed);

            // 4. Apply to Shader
            _effectMaterial.SetFloat(SpeedProp, _currentIntensity);
            _effectMaterial.SetFloat(AlphaProp, _currentAlpha);

            // 5. Handle Mask Size via Curve
            float evaluatedMaskSize = _maskSizeCurve.Evaluate(_currentIntensity);
            _effectMaterial.SetFloat(MaskSizeProp, evaluatedMaskSize);
        }

        private void OnDisable()
        {
            if (_effectMaterial != null)
            {
                _effectMaterial.SetFloat(SpeedProp, 0);
                _effectMaterial.SetFloat(AlphaProp, 1); // Reset alpha to fully visible
                _effectMaterial.SetFloat(MaskSizeProp, _maskSizeCurve.Evaluate(0));
            }
        }
    }
}