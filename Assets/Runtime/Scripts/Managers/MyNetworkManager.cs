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
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += ServerHasStarted;
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
            NetworkManager.Singleton.SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }
    }
}