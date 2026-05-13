using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkBaseRuntime
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpawnManager _spawnManager;

        [Header("Settings")]
        [SerializeField] private string _gameplaySceneName = "TheGame";

        private bool _isInitialized;

        private void OnEnable()
        {
            // Subscribe early — NetworkManager may already be running
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
                Debug.Log("[GameManager] Subscribed to SceneManager.OnSceneEvent in OnEnable.");
            }
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
                Debug.Log("[GameManager] Unsubscribed from SceneManager.OnSceneEvent.");
            }
        }

        private void Start()
        {
            Debug.Log($"[GameManager] Start() — Scene: {gameObject.scene.name}");

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[GameManager] ERROR: NetworkManager.Singleton is NULL!");
                return;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.Log("[GameManager] Not the server, skipping spawn logic.");
                return;
            }

            if (_spawnManager == null)
            {
                Debug.LogError("[GameManager] ERROR: _spawnManager is NULL! Assign it in the Inspector.");
                return;
            }

            // Spawn the host player immediately — the host has already loaded this scene
            ulong hostClientId = NetworkManager.Singleton.LocalClientId;
            Debug.Log($"[GameManager] Spawning host player (ClientId={hostClientId})...");
            _spawnManager.SpawnPlayer(hostClientId);
            _isInitialized = true;
        }

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            Debug.Log($"[GameManager] SceneEvent: Type={sceneEvent.SceneEventType}, Scene={sceneEvent.SceneName}, ClientId={sceneEvent.ClientId}");

            // Only the server should handle spawning
            if (!NetworkManager.Singleton.IsServer) return;

            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                if (sceneEvent.SceneName == _gameplaySceneName)
                {
                    // Don't double-spawn the host — they were already spawned in Start()
                    if (sceneEvent.ClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        Debug.Log($"[GameManager] Skipping host (ClientId={sceneEvent.ClientId}), already spawned in Start().");
                        return;
                    }

                    Debug.Log($"[GameManager] Client {sceneEvent.ClientId} finished loading '{_gameplaySceneName}'. Spawning player...");
                    _spawnManager.SpawnPlayer(sceneEvent.ClientId);
                }
                else
                {
                    Debug.Log($"[GameManager] Client {sceneEvent.ClientId} loaded '{sceneEvent.SceneName}' — not gameplay scene, ignoring.");
                }
            }
        }
    }
}