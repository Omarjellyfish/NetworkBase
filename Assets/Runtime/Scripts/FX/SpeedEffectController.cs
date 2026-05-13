using UnityEngine;

namespace NetworkBaseRuntime
{
    public class SpeedEffectController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Material _effectMaterial;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PlayerStats _stats;

        [Header("Effect Settings")]
        [Tooltip("How fast the shader effect ramps up when accelerating")]
        [SerializeField] private float _lerpSpeedUp = 5f;

        [Tooltip("How fast the shader effect fades out when decelerating")]
        [SerializeField] private float _lerpSpeedDown = 2f;

        [Tooltip("The shader's Speed value when the vehicle is at MaxSpeed")]
        [SerializeField] private float _maxShaderSpeed = 2f;

        [Header("Visibility Tuning")]
        [Tooltip("Speeds below this will be treated as absolute zero to prevent physics jitter.")]
        [SerializeField] private float _speedDeadzone = 0.1f;

        [Tooltip("Mask size when stopped. Set higher than 1 (e.g., 1.5) to guarantee invisibility.")]
        [SerializeField] private float _idleMaskSize = 1.5f;

        [Tooltip("Mask size when going at MaxSpeed.")]
        [SerializeField] private float _maxSpeedMaskSize = 0.7f;

        [Header("Color Settings")]
        [Tooltip("Color of the effect when stopped.")]
        [ColorUsage(showAlpha: true, hdr: true)]
        [SerializeField] private Color _idleColor = new Color(1f, 1f, 1f, 0f); // Defaults to transparent white

        [Tooltip("Color of the effect when at MaxSpeed.")]
        [ColorUsage(showAlpha: true, hdr: true)]
        [SerializeField] private Color _maxSpeedColor = Color.white;

        // Property IDs
        private static readonly int SpeedProp = Shader.PropertyToID("_Speed");
        private static readonly int MaskSizeProp = Shader.PropertyToID("_MaskSize");
        private static readonly int SpeedColorEffectProp = Shader.PropertyToID("_SpeedColorEffect");

        private float _currentNormalizedSpeed;
        private PlayerInputHandler _inputHandler;

        private void Awake()
        {
            _inputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            // Only run on the locally owned player — prevents both instances
            // writing to the same shared material
            if (!_inputHandler.enabled) return;
            if (_effectMaterial == null || _rb == null || _stats == null) return;

            // 1. Calculate horizontal speed
            float horizontalSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;

            // Deadzone check. If we are barely moving, force speed to 0.
            if (horizontalSpeed < _speedDeadzone)
            {
                horizontalSpeed = 0f;
            }

            // 2. Get normalized speed (0 at standstill, 1 at MaxSpeed)
            float targetNormalizedSpeed = Mathf.InverseLerp(0f, _stats.MaxSpeed, horizontalSpeed);

            // 3. Determine which lerp speed to use based on whether we are speeding up or slowing down
            float activeLerpSpeed = targetNormalizedSpeed > _currentNormalizedSpeed ? _lerpSpeedUp : _lerpSpeedDown;

            // 4. Smooth the transition using the chosen lerp speed
            _currentNormalizedSpeed = Mathf.Lerp(_currentNormalizedSpeed, targetNormalizedSpeed, Time.deltaTime * activeLerpSpeed);

            // Force _currentNormalizedSpeed to exactly 0 if it gets extremely close, 
            // bypassing the asymptotic tail of Mathf.Lerp
            if (_currentNormalizedSpeed < 0.001f)
            {
                _currentNormalizedSpeed = 0f;
            }

            // 5. Calculate Shader Values
            float currentMaskSize = Mathf.Lerp(_idleMaskSize, _maxSpeedMaskSize, _currentNormalizedSpeed);
            float currentShaderSpeed = Mathf.Lerp(0f, _maxShaderSpeed, _currentNormalizedSpeed);
            Color currentShaderColor = Color.Lerp(_idleColor, _maxSpeedColor, _currentNormalizedSpeed);

            // 6. Apply to Shader
            _effectMaterial.SetFloat(SpeedProp, currentShaderSpeed);
            _effectMaterial.SetFloat(MaskSizeProp, currentMaskSize);
            _effectMaterial.SetColor(SpeedColorEffectProp, currentShaderColor);
        }

        private void OnDisable()
        {
            if (_effectMaterial != null)
            {
                // Reset to default
                _effectMaterial.SetFloat(SpeedProp, 0f);
                _effectMaterial.SetFloat(MaskSizeProp, _idleMaskSize);
                _effectMaterial.SetColor(SpeedColorEffectProp, _idleColor);
            }
        }
    }
}