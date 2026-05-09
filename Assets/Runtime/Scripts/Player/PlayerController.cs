using UnityEngine;
using UnityEngine.InputSystem;
namespace NetworkBaseRuntime
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;

        private Rigidbody _rb;
        private GroundCheck _groundCheck;
        private PlayerMovement _movement;
        private JumpController _jump;
        private GravityController _gravity;

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

            _groundCheck.Init(_stats);
            _movement.Init(_stats, _groundCheck);
            _jump.Init(_stats, _groundCheck);
            _gravity.Init(_stats, _groundCheck, _jump);

            // Set up input
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Jump.performed += _ =>
            {
                _jumpDown = true;
                _jump.RequestJump(_time);
            };
            _inputActions.Player.Jump.canceled += _ => _jumpHeld = false;
            _inputActions.Player.Jump.performed += _ => _jumpHeld = true;

            _inputActions.Player.Jump.performed += _ =>
            {
                _jumpDown = true;
                _jump.RequestJump(_time);
                Debug.Log($"[Input] Jump performed | isGrounded: {_groundCheck.IsGrounded}");
            };
            _inputActions.Player.Jump.canceled += _ => _jumpHeld = false;
            _inputActions.Player.Jump.performed += _ => _jumpHeld = true;
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
            _groundCheck.Check(_time);
            Debug.Log($"[Frame] isGrounded: {_groundCheck.IsGrounded} | frameVelocity: {_frameVelocity}");

            _frameVelocity = _jump.HandleJump(_frameVelocity, _jumpHeld, _time);
            _frameVelocity = _movement.HandleDirection(_frameVelocity, _moveInput);
            _frameVelocity = _gravity.HandleGravity(_frameVelocity);

            _rb.linearVelocity = _frameVelocity;
        }
    }
}