using Unity.Netcode;
using UnityEngine;

namespace NetworkBaseRuntime
{
    public class SpawnManager : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        private int _currentSpawnIndex;

        private void Awake()
        {
            Debug.Log($"[SpawnManager] Awake() called. Scene: {gameObject.scene.name}");
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"[SpawnManager] OnNetworkSpawn() — IsServer={IsServer}, IsHost={IsHost}, IsClient={IsClient}, IsSpawned={IsSpawned}");
            Debug.Log($"[SpawnManager] Prefab assigned: {(_playerPrefab != null ? _playerPrefab.name : "NULL")}");
            Debug.Log($"[SpawnManager] Spawn points: {(_spawnPoints != null ? _spawnPoints.Length.ToString() : "NULL")}");
        }

        public void SpawnPlayer(ulong clientId)
        {
            Debug.Log($"[SpawnManager] SpawnPlayer() called for ClientId={clientId}");
            Debug.Log($"[SpawnManager] State check — IsServer={IsServer}, IsSpawned={IsSpawned}, NetworkObjectId={NetworkObjectId}");

            // Security check: Only the server can spawn objects
            if (!IsServer)
            {
                Debug.LogError($"[SpawnManager] BLOCKED: IsServer is FALSE. SpawnManager's NetworkObject may not be spawned yet.");
                return;
            }

            // Validate prefab
            if (_playerPrefab == null)
            {
                Debug.LogError("[SpawnManager] ERROR: _playerPrefab is NULL! Assign it in the Inspector.");
                return;
            }

            // 1. Calculate Spawn Position
            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                Transform currentPoint = _spawnPoints[_currentSpawnIndex % _spawnPoints.Length];
                spawnPosition = currentPoint.position;
                spawnRotation = currentPoint.rotation;
                _currentSpawnIndex++;
                Debug.Log($"[SpawnManager] Using spawn point index {_currentSpawnIndex - 1}, position={spawnPosition}");
            }
            else
            {
                Debug.LogWarning("[SpawnManager] No spawn points assigned, using Vector3.zero");
            }

            // 2. Instantiate the physical GameObject on the server
            Debug.Log($"[SpawnManager] Instantiating prefab '{_playerPrefab.name}' at {spawnPosition}...");
            NetworkObject playerInstance = Instantiate(_playerPrefab, spawnPosition, spawnRotation);

            if (playerInstance == null)
            {
                Debug.LogError("[SpawnManager] ERROR: Instantiate returned null!");
                return;
            }

            // 3. Spawn it across the network and assign it as this client's player object
            Debug.Log($"[SpawnManager] Calling SpawnAsPlayerObject(clientId={clientId})...");
            playerInstance.SpawnAsPlayerObject(clientId, destroyWithScene: true);
            Debug.Log($"[SpawnManager] SUCCESS: Player spawned for ClientId={clientId}, NetworkObjectId={playerInstance.NetworkObjectId}");
        }
    }
}