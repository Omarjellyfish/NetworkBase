using UnityEngine;
using UnityEngine.InputSystem;
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

        [Header("Camera References")]
        private PlayerLook _look;
        [SerializeField] private Animator _cameraAnimator;
        [SerializeField] private Transform _cameraRoot; 
        private Vector2 _lookInput; 
        public PlayerState CurrentState { get; private set; }

        // New Input System
        private PlayerInputActions _inputActions;
        private Vector2 _moveInput;
        private bool _jumpHeld;
        private bool _jumpDown;

        private Vector3 _frameVelocity;
        private float _time;

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


            _groundCheck.Init(_stats);
            _movement.Init(_stats, _groundCheck);
            _jump.Init(_stats, _groundCheck, _wallCheck);
            _gravity.Init(_stats, _groundCheck, _jump);

            _wallRun.Init(_stats, _groundCheck, _wallCheck);

            _look = GetComponent<PlayerLook>();

            _look.Init(_stats, _cameraRoot);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Set up input
            _inputActions = new PlayerInputActions();

            _inputActions.Player.Jump.performed += _ =>
            {
                _jumpDown = true;
                _jumpHeld = true;
                _jump.RequestJump(_time);
                Debug.Log($"[Input] Jump performed | isGrounded: {_groundCheck.IsGrounded}");
            };

            _inputActions.Player.Jump.canceled += _ => _jumpHeld = false;

        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void GatherInput()
        {
            _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            _lookInput = _inputActions.Player.Look.ReadValue<Vector2>(); 
            if (_stats.SnapInput)
            {
                _moveInput.x = Mathf.Abs(_moveInput.x) < _stats.HorizontalDeadZoneThreshold
                    ? 0 : Mathf.Sign(_moveInput.x);
                _moveInput.y = Mathf.Abs(_moveInput.y) < _stats.VerticalDeadZoneThreshold
                    ? 0 : Mathf.Sign(_moveInput.y);
            }

            // DEBUG
            if (_moveInput != Vector2.zero)
                Debug.Log($"[Input] Move: {_moveInput}");

            _jumpDown = false;
        }

       private void FixedUpdate()
        {
            _time += Time.fixedDeltaTime;
            _groundCheck.Check(_time);
            _wallCheck.Check(); // Added the wall check call here

            UpdateState();

            // Pass velocity through the pipeline
            _frameVelocity = _rb.linearVelocity;

            _frameVelocity = _jump.HandleJump(_frameVelocity, _jumpHeld, _time);
            _frameVelocity = _movement.HandleDirection(_frameVelocity, _moveInput);

            // Only wall run if the state allows it
            if (CurrentState == PlayerState.WallRunning)
            {
                _frameVelocity = _wallRun.HandleWallRun(_frameVelocity, _moveInput);
            }

            _frameVelocity = _gravity.HandleGravity(_frameVelocity);
            _rb.linearVelocity = _frameVelocity;
            if (_cameraAnimator != null)
            {
                _cameraAnimator.SetInteger("State", (int)CurrentState);
            }
        }

        private void LateUpdate()
        {
            _look.HandleLook(_lookInput);
        }
        private void UpdateState()
        {
            if (_wallCheck.IsWall && _moveInput.y > 0) // Wall check first, just in case you skim the floor
            {
                CurrentState = PlayerState.WallRunning;
            }
            else if (_groundCheck.IsGrounded)
            {
                if (_moveInput.magnitude > 0)
                {
                    // Calculate horizontal speed to see if we are sprinting
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
    }
}