using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NetworkBaseRuntime
{
    /// <summary>
    /// Reads input from the New Input System and exposes it as simple properties.
    /// As the only NetworkBehaviour on the player, it gates input behind IsOwner
    /// and disables the camera/audio for non-owners.
    /// </summary>
    public class PlayerInputHandler : NetworkBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        [SerializeField] private Transform _cameraRoot;

        private PlayerInputActions _inputActions;

        /// <summary>Fired on the local client when our owned player spawns. Passes the camera root.</summary>
        public static event System.Action<Transform> OnLocalPlayerSpawned;

        /// <summary>Fired on the local client when our owned player despawns.</summary>
        public static event System.Action OnLocalPlayerDespawned;

        /// <summary>Fired instantly when jump is pressed. Subscribe to call RequestJump with correct timing.</summary>
        public event System.Action OnJumpPerformed;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsJumpHeld { get; private set; }

        private void Awake()
        {
            _inputActions = new PlayerInputActions();

            _inputActions.Player.Jump.performed += _ =>
            {
                IsJumpHeld = true;
                OnJumpPerformed?.Invoke();
            };

            _inputActions.Player.Jump.canceled += _ => IsJumpHeld = false;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _inputActions.Enable();

            OnLocalPlayerSpawned?.Invoke(_cameraRoot);
        }

        public override void OnNetworkDespawn()
        {
            _inputActions.Disable();

            if (IsOwner)
            {
                OnLocalPlayerDespawned?.Invoke();
            }
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }

        private void Update()
        {
            Vector2 rawMove = _inputActions.Player.Move.ReadValue<Vector2>();
            LookInput = _inputActions.Player.Look.ReadValue<Vector2>();

            if (_stats.SnapInput)
            {
                rawMove.x = Mathf.Abs(rawMove.x) < _stats.HorizontalDeadZoneThreshold
                    ? 0 : Mathf.Sign(rawMove.x);
                rawMove.y = Mathf.Abs(rawMove.y) < _stats.VerticalDeadZoneThreshold
                    ? 0 : Mathf.Sign(rawMove.y);
            }

            MoveInput = rawMove;
        }


    }
}
