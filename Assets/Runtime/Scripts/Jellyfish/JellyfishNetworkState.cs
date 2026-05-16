using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class JellyfishNetworkState : NetworkBehaviour
{
    public enum JellyfishState { Wander, Chase }

    [Header("Synced State")]
    // This NetworkVariable automatically beams the state from the Server to all Clients
    public NetworkVariable<JellyfishState> currentState = new NetworkVariable<JellyfishState>(
        JellyfishState.Wander,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        // If this is a client, turn off local physics so the NetworkTransform can drag it smoothly
        if (!IsServer && rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void OnGUI()
    {
        if (!IsSpawned) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 28;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperRight;

        // Read the synchronized .Value so it updates on both Host and Client screens!
        style.normal.textColor = currentState.Value == JellyfishState.Chase ? Color.red : Color.cyan;

        Rect rect = new Rect(Screen.width - 220, 20, 200, 50);
        GUI.Label(rect, "STATE: " + currentState.Value.ToString().ToUpper(), style);
    }
}