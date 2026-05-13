using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace NetworkBaseRuntime
{
    public class MyNetworkManager : MonoBehaviour
    {
        [Header("Scene Management")]
        [SerializeField] private string targetSceneName = "GameLobby";

        private void Start()
        {
            Debug.Log("[MyNetworkManager] Start() called.");
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += ServerHasStarted;
                Debug.Log("[MyNetworkManager] Subscribed to OnServerStarted.");
            }
            else
            {
                Debug.LogError("[MyNetworkManager] NetworkManager.Singleton is NULL in Start()!");
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= ServerHasStarted;
            }
        }

        private void ServerHasStarted()
        {
            Debug.Log($"[MyNetworkManager] ServerHasStarted() — Loading scene '{targetSceneName}'");
            NetworkManager.Singleton.SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }
    }
}