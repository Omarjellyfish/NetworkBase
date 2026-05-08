using UnityEngine;
using Unity.Netcode;
namespace NetworkBaseRuntime
{
    public class SpawnManager : MonoBehaviour
    {
        public void SpawnPlayer(ulong clientId , GameObject playerPrefab,Vector3 SpawnPosition)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                GameObject playerInstance = Instantiate(playerPrefab);
                NetworkManager.Singleton.SpawnManager.Spawn(playerInstance, clientId);
            }
        }
    }
}
