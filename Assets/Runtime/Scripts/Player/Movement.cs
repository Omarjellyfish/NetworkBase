using System;
using UnityEngine;

namespace NetworkBaseRuntime
{
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        private Vector3 _movement;
        private Rigidbody _rb;

        [Header("Movement Attributes")]
        [SerializeField] private float accelerationForce = 50f;
        [SerializeField] private float max_speed = 5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float deaccleration = 5f;
        [SerializeField] private float wallJumpForce = 5f;
        [SerializeField] private bool isJumping;

        [Header("Ground Check Attributes")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private bool isGrounded;

        [Header("Wall Check Attributes")]
        [SerializeField] private bool isWall;
        [SerializeField] private float wallCheckDistance = 0.1f;
        [SerializeField] private LayerMask wallLayer;

        // Variables to safely pass input from Update to FixedUpdate
        private Vector2 _input;
        private bool _jumpRequested;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            _input = GetInput();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpRequested = true;
                Debug.Log("Spacebar pressed! Jump requested.");
            }
        }

        void FixedUpdate()
        {
            CheckGround();
            CheckWall();
            HandleMovement();
            HandleJump();
        }

        private void HandleJump()
        {
            if (_jumpRequested)
            {
                // Log the state of the booleans when a jump is attempted
                Debug.Log($"Attempting Jump. isGrounded: {isGrounded} | isWall: {isWall}");

                if (isGrounded)
                {
                    _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    isJumping = true;
                    Debug.Log("Success: Performed Ground Jump!");
                }
                else if (isWall)
                {
                    Vector3 wallJumpDirection = -transform.forward + Vector3.up;
                    _rb.AddForce(wallJumpDirection.normalized * wallJumpForce, ForceMode.Impulse);
                    isJumping = true;
                    Debug.Log("Success: Performed Wall Jump!");
                }
                else
                {
                    Debug.LogWarning("Failed: Cannot jump because the player is neither grounded nor on a wall. Check your Raycast distances and LayerMasks!");
                }

                _jumpRequested = false;
            }
        }

        void HandleWallRun()
        {
            if (isWall && !isGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce * 0.5f, ForceMode.Acceleration);
            }
        }

        private void CheckWall()
        {
            isWall = Physics.Raycast(transform.position, transform.forward, wallCheckDistance, wallLayer);
        }

        private void CheckGround()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        }

        void HandleMovement()
        {
            _movement = new Vector3(_input.x, 0, _input.y);

            if (_movement != Vector3.zero)
            {
                _rb.AddForce(_movement.normalized * accelerationForce);

                if (_rb.linearVelocity.magnitude > max_speed)
                {
                    _rb.linearVelocity = _rb.linearVelocity.normalized * max_speed;
                }
            }
            else
            {
                _rb.AddForce(_rb.linearVelocity * -deaccleration);
            }
        }

        Vector2 GetInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            return new Vector2(horizontal, vertical);
        }
    }
}