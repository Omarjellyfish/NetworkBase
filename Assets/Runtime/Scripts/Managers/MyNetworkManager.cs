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
                NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= ServerHasStarted;
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }
        }


        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;
        }
        private void ServerHasStarted()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }
    }
}