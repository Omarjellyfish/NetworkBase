using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
namespace NetworkBaseRuntime
{
    public class MyNetworkManager: NetworkManager
    {
        public static MyNetworkManager Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += ServerHasStarted;
        }
        private void ServerHasStarted()
        {
            Singleton.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
        }
    }
}
