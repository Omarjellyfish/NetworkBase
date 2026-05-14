using UnityEngine;

namespace NetworkBaseRuntime
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;

        [Header("Required Components")]
        private Rigidbody _rb;
        private GroundCheck _groundCheck;
        private PlayerMovement _movement;
        private JumpController _jump;
        private GravityController _gravity;
        private WallRun _wallRun;
        private WallCheck _wallCheck;
        private PlayerInputHandler _inputHandler;

        [Header("Camera References")]
        private PlayerLook _look;
        [SerializeField] private Transform _cameraRoot;
        private Animator _cameraAnimator;

        public PlayerState CurrentState { get; private set; }

        private Vector3 _frameVelocity;
        private float _time;

        private bool _isCameraAnimatorCached;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            _groundCheck = GetComponent<GroundCheck>();
            _movement = GetComponent<PlayerMovement>();
            _jump = GetComponent<JumpController>();
            _gravity = GetComponent<GravityController>();

            _wallRun = GetComponent<WallRun>();
            _wallCheck = GetComponent<WallCheck>();
            _inputHandler = GetComponent<PlayerInputHandler>();

            _groundCheck.Init(_stats);
            _movement.Init(_stats, _groundCheck);
            _jump.Init(_stats, _groundCheck, _wallCheck);
            _gravity.Init(_stats, _groundCheck, _jump);

            _wallRun.Init(_stats, _groundCheck, _wallCheck);

            _look = GetComponent<PlayerLook>();
            _look.Init(_stats, _cameraRoot);
        }

        private void OnEnable()
        {
            _inputHandler.OnJumpPerformed += HandleJumpPerformed;
        }

        private void OnDisable()
        {
            _inputHandler.OnJumpPerformed -= HandleJumpPerformed;
        }

        private void HandleJumpPerformed()
        {
            _jump.RequestJump(_time);
        }

        private void Update()
        {
            // Input handler disables itself for non-owners, so skip when inactive
            if (!_inputHandler.enabled) return;

            _time += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!_inputHandler.enabled) return;

            _time += Time.fixedDeltaTime;
            _groundCheck.Check(_time);
            _wallCheck.Check();

            UpdateState();

            Vector2 moveInput = _inputHandler.MoveInput;

            // Pass velocity through the pipeline
            _frameVelocity = _rb.linearVelocity;

            _frameVelocity = _jump.HandleJump(_frameVelocity, _inputHandler.IsJumpHeld, _time);
            _frameVelocity = _movement.HandleDirection(_frameVelocity, moveInput);

            // Only wall run if the state allows it
            if (CurrentState == PlayerState.WallRunning)
            {
                _frameVelocity = _wallRun.HandleWallRun(_frameVelocity, moveInput);
            }

            _frameVelocity = _gravity.HandleGravity(_frameVelocity);
            _rb.linearVelocity = _frameVelocity;

            // Lazy-cache the scene camera animator (runs once)
            if (!_isCameraAnimatorCached)
            {
                _cameraAnimator = FindAnyObjectByType<AttachCameraToPlayer>()
                    ?.GetComponentInChildren<Animator>();
                _isCameraAnimatorCached = true;
            }

            if (_cameraAnimator != null)
            {
                _cameraAnimator.SetInteger("State", (int)CurrentState);
            }
        }

        private void LateUpdate()
        {
            if (!_inputHandler.enabled) return;

            _look.HandleLook(_inputHandler.LookInput);
        }

        private void UpdateState()
        {
            Vector2 moveInput = _inputHandler.MoveInput;

            if (_wallCheck.IsWall && moveInput.y > 0)
            {
                CurrentState = PlayerState.WallRunning;
            }
            else if (_groundCheck.IsGrounded)
            {
                if (moveInput.magnitude > 0)
                {
                    float horizontalSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;

                    if (horizontalSpeed > _stats.MaxSpeed * 0.8f)
                    {
                        CurrentState = PlayerState.Sprinting;
                    }
                    else
                    {
                        CurrentState = PlayerState.Walking;
                    }
                }
                else
                {
                    CurrentState = PlayerState.Idling;
                }
            }
            else
            {
                CurrentState = PlayerState.InAir;
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!Application.isPlaying || !_inputHandler.enabled) return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(20, 20, 300, 50), $"State: {CurrentState}", style);
        }
#endif
    }
}