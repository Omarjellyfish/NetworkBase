using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using NetworkBaseNetwork;

namespace NetworkBaseRuntime
{
 
    public class LobbyToNetworkManager : MonoBehaviour
    {
        private void OnEnable()
        {
            // Subscribe to the Matchmaking events from your Network assembly
            LobbyServiceManager.OnLobbyCreated += HandleLobbyCreated;
            LobbyServiceManager.OnLobbyJoined += HandleLobbyJoined;
            LobbyServiceManager.OnLeftLobby += HandleLeftLobby;
        }

        private void OnDisable()
        {
            LobbyServiceManager.OnLobbyCreated -= HandleLobbyCreated;
            LobbyServiceManager.OnLobbyJoined -= HandleLobbyJoined;
            LobbyServiceManager.OnLeftLobby -= HandleLeftLobby;
        }

        private void HandleLobbyCreated(Lobby lobby)
        {
            Debug.Log("LobbyToNetwork (Runtime): Lobby created, starting NGO as Host.");

     
            NetworkManager.Singleton.StartHost();
        }

        private void HandleLobbyJoined(Lobby lobby)
        {
            Debug.Log("LobbyToNetwork (Runtime): Lobby joined, starting NGO as Client.");

            NetworkManager.Singleton.StartClient();
        }

        private void HandleLeftLobby()
        {
            Debug.Log("LobbyToNetwork (Runtime): Left lobby, shutting down NGO.");

            NetworkManager.Singleton.Shutdown();
        }
    }
}