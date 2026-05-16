using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TentacleEndpoint : MonoBehaviour
{
    [Header("References")]
    public Transform mainBody;

    [Header("Boids & Leash Settings")]
    public float maxLeashDistance = 10f;
    public float moveSpeed = 8f;
    public float wanderStrength = 2f;

    [Header("Separation Settings")]
    public float separationRadius = 1.5f;
    public float separationStrength = 5f;
    public LayerMask tentacleLayer;

    [Header("Resting Settings")]
    public float restingSpread = 3.5f;
    public float restingDepth = 2.5f;

    [Header("Debug")]
    // Added SerializeField so you can see who this tentacle is chasing in the inspector!
    [SerializeField] private Transform currentTarget;

    private Rigidbody rb;
    private Vector3 frameVelocity;

    private float randomOffset;
    private Vector3 uniqueRestOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        randomOffset = Random.Range(0f, 100f);

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(restingSpread * 0.5f, restingSpread);

        uniqueRestOffset = new Vector3(
            randomDir.x * randomDist,
            -restingDepth + Random.Range(-0.5f, 0.5f),
            randomDir.y * randomDist
        );
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }

    private void FixedUpdate()
    {
        frameVelocity = rb.linearVelocity; // Updated to linearVelocity
        Vector3 steeringForce = Vector3.zero;

        if (currentTarget != null)
        {
            steeringForce += CalculateCohesion(currentTarget.position);
            steeringForce += CalculateWander();
        }
        else
        {
            Vector3 restPosition = mainBody.position + uniqueRestOffset;
            steeringForce += CalculateCohesion(restPosition);
            steeringForce += CalculateWander() * 0.5f;
        }

        steeringForce += CalculateSeparation();
        steeringForce += CalculateContainment();

        frameVelocity = Vector3.Lerp(frameVelocity, steeringForce, Time.fixedDeltaTime * 5f);
        rb.linearVelocity = frameVelocity;
    }

    private Vector3 CalculateCohesion(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        return direction * moveSpeed;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 repelForce = Vector3.zero;
        Collider[] nearbyTentacles = Physics.OverlapSphere(transform.position, separationRadius, tentacleLayer);

        foreach (Collider col in nearbyTentacles)
        {
            if (col.gameObject != gameObject)
            {
                Vector3 directionAway = transform.position - col.transform.position;
                float distance = directionAway.magnitude;
                if (distance > 0.1f)
                {
                    repelForce += directionAway.normalized / distance;
                }
            }
        }

        return repelForce * separationStrength;
    }

    private Vector3 CalculateContainment()
    {
        Vector3 offsetFromBody = transform.position - mainBody.position;
        float distance = offsetFromBody.magnitude;

        if (distance > maxLeashDistance)
        {
            return -offsetFromBody.normalized * (moveSpeed * 1.5f);
        }

        return Vector3.zero;
    }

    private Vector3 CalculateWander()
    {
        float noiseX = Mathf.PerlinNoise(Time.time * 2f, randomOffset) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(Time.time * 2f + 10f, randomOffset) * 2f - 1f;
        float noiseZ = Mathf.PerlinNoise(Time.time * 2f + 20f, randomOffset) * 2f - 1f;

        return new Vector3(noiseX, noiseY, noiseZ).normalized * wanderStrength;
    }
}