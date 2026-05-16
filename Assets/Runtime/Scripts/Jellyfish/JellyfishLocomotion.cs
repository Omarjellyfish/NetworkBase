using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class JellyfishLocomotion : NetworkBehaviour
{
    [Header("Brain & Network Links")]
    public JellyfishNetworkState networkState;
    public TentacleCoordinator coordinator;

    [Header("Movement Settings")]
    public float wanderSpeed = 4f;
    public float chaseSpeed = 12f;
    public float rotationSpeed = 5f;

    [Header("Buoyancy & Ground Settings")]
    [SerializeField] float groundRaycastLength = 80f;
    public float targetAltitude = 3f;
    public float buoyancyForce = 5f;
    public float minGroundClearance = 1.5f;
    public float emergencyPushForce = 15f;

    [Tooltip("How high off the ground the jellyfish aims when swarming players.")]
    public float attackDiveHeight = 1.5f;

    public LayerMask groundLayer;

    [Header("Steering Settings")]
    public float whiskerLength = 4f;
    public float avoidanceForce = 10f;

    private Rigidbody rb;
    private Vector3 frameVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        // Only the Server calculates movement
        if (!IsServer) return;

        // 1. The Server updates the Network State based on targets
        if (coordinator != null && coordinator.HasTargets())
        {
            networkState.currentState.Value = JellyfishNetworkState.JellyfishState.Chase;
        }
        else
        {
            networkState.currentState.Value = JellyfishNetworkState.JellyfishState.Wander;
        }

        frameVelocity = rb.linearVelocity;
        Vector3 desiredDirection = Vector3.zero;

        // 2. Base Movement (Read from the synced state)
        if (networkState.currentState.Value == JellyfishNetworkState.JellyfishState.Chase)
        {
            // Get the center point between the players
            Vector3 targetPos = coordinator.GetTargetCenter();

            // Lift the target position off the floor using our exposed variable
            targetPos.y += attackDiveHeight;

            desiredDirection = (targetPos - transform.position).normalized * chaseSpeed;
        }
        else
        {
            desiredDirection = transform.forward * wanderSpeed;
        }

        // 3. Wall Avoidance
        desiredDirection += CalculateWallAvoidance();

        // 4. Ground Interaction
        desiredDirection = ApplyGroundLogic(desiredDirection);

        // 5. Rotation and Velocity applied
        if (desiredDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection.normalized);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }

        frameVelocity = Vector3.Lerp(frameVelocity, desiredDirection, Time.fixedDeltaTime * 4f);
        rb.linearVelocity = frameVelocity;
    }

    private Vector3 CalculateWallAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit frontHit, whiskerLength, groundLayer))
            avoidance += frontHit.normal * avoidanceForce;

        if (Physics.Raycast(transform.position, transform.forward - transform.right, out RaycastHit leftHit, whiskerLength, groundLayer))
            avoidance += leftHit.normal * avoidanceForce;

        if (Physics.Raycast(transform.position, transform.forward + transform.right, out RaycastHit rightHit, whiskerLength, groundLayer))
            avoidance += rightHit.normal * avoidanceForce;

        return avoidance;
    }

    private Vector3 ApplyGroundLogic(Vector3 currentDesiredDirection)
    {
        Debug.DrawRay(transform.position, Vector3.down * groundRaycastLength, Color.red);

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundRaycastLength, groundLayer))
        {
            float distanceToGround = hit.distance;

            if (networkState.currentState.Value == JellyfishNetworkState.JellyfishState.Wander)
            {
                float altitudeDifference = targetAltitude - distanceToGround;
                currentDesiredDirection.y = altitudeDifference * buoyancyForce;
            }
            else if (networkState.currentState.Value == JellyfishNetworkState.JellyfishState.Chase)
            {
                if (distanceToGround < minGroundClearance)
                {
                    float panicFactor = 1f - (distanceToGround / minGroundClearance);
                    currentDesiredDirection.y += (panicFactor * emergencyPushForce);
                }
            }
        }
        else
        {
            if (networkState.currentState.Value == JellyfishNetworkState.JellyfishState.Wander)
            {
                currentDesiredDirection.y = -2f;
            }
        }

        return currentDesiredDirection;
    }
}