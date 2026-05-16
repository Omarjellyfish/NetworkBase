using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class TentacleCoordinator : NetworkBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 15f;
    public LayerMask targetLayer;

    [Header("Appendages")]
    public TentacleEndpoint[] tentacles;

    [Header("Debug: Current Targets")]
    [SerializeField] private List<Transform> visibleTargets = new List<Transform>();

    private float detectionTimer = 0f;
    public float detectionInterval = 0.5f;

    private List<ITargetable> internalTargets = new List<ITargetable>();

    private void Update()
    {
        // Only the Server scans for targets
        if (!IsServer) return;

        detectionTimer += Time.deltaTime;
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            AssignTargets();
        }
    }

    private void AssignTargets()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);

        internalTargets.Clear();
        visibleTargets.Clear();

        foreach (Collider hit in hits)
        {
            ITargetable target = hit.GetComponentInParent<ITargetable>();
            if (target != null && !internalTargets.Contains(target))
            {
                internalTargets.Add(target);
                visibleTargets.Add(target.GetTransform());
            }
        }

        // Prepare an array of IDs to send to the clients
        ulong[] targetNetworkIds = new ulong[tentacles.Length];

        if (internalTargets.Count == 0)
        {
            for (int i = 0; i < tentacles.Length; i++)
            {
                tentacles[i].SetTarget(null);
                targetNetworkIds[i] = 0; // 0 means no target
            }
        }
        else
        {
            for (int i = 0; i < tentacles.Length; i++)
            {
                ITargetable assignedTarget = internalTargets[i % internalTargets.Count];
                tentacles[i].SetTarget(assignedTarget.GetTransform());

                // Grab the NetworkObject ID of the player to send to the clients
                NetworkObject netObj = assignedTarget.GetTransform().GetComponentInParent<NetworkObject>();
                targetNetworkIds[i] = netObj != null ? netObj.NetworkObjectId : 0;
            }
        }

        // Broadcast the IDs to all connected clients!
        UpdateClientTentaclesClientRpc(targetNetworkIds);
    }

    // --- NEW: This runs on the Clients to sync their visuals ---
    [ClientRpc]
    private void UpdateClientTentaclesClientRpc(ulong[] targetIds)
    {
        // The server already did this locally, so skip it to avoid double-processing
        if (IsServer) return;

        for (int i = 0; i < tentacles.Length; i++)
        {
            if (targetIds[i] == 0)
            {
                tentacles[i].SetTarget(null);
            }
            else
            {
                // Find the player object using the ID the server sent
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetIds[i], out NetworkObject targetObj))
                {
                    tentacles[i].SetTarget(targetObj.transform);
                }
            }
        }
    }

    public bool HasTargets()
    {
        return internalTargets.Count > 0;
    }

    public Vector3 GetTargetCenter()
    {
        if (internalTargets.Count == 0) return transform.position;

        Vector3 center = Vector3.zero;
        foreach (var target in internalTargets)
        {
            center += target.GetTransform().position;
        }
        return center / internalTargets.Count;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}