using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using NetworkBaseNetwork;
using Unity.Services.Authentication;
using System.Collections.Generic;

namespace NetworkBaseRuntime
{
    public class LobbyToNetworkManager : MonoBehaviour
    {
        private Dictionary<string, ulong> ugsToNetcodeMap = new Dictionary<string, ulong>();

        private void OnEnable()
        {
            LobbyServiceManager.OnLobbyCreated += HandleLobbyCreated;
            LobbyServiceManager.OnLobbyJoined += HandleLobbyJoined;
            LobbyServiceManager.OnLeftLobby += HandleLeftLobby;
            LobbyServiceManager.OnPlayerKicked += HandlePlayerKicked;
        }

        private void OnDisable()
        {
            LobbyServiceManager.OnLobbyCreated -= HandleLobbyCreated;
            LobbyServiceManager.OnLobbyJoined -= HandleLobbyJoined;
            LobbyServiceManager.OnLeftLobby -= HandleLeftLobby;
            LobbyServiceManager.OnPlayerKicked -= HandlePlayerKicked;
        }

        private void HandleLobbyCreated(Lobby lobby)
        {
            Debug.Log("LobbyToNetwork (Local): Lobby created, starting NGO as Host.");

            // 1. Setup connection approval to map IDs
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            ugsToNetcodeMap.Clear();

            // 2. Start the Host locally (defaults to 127.0.0.1)
            NetworkManager.Singleton.StartHost();
        }

        private void HandleLobbyJoined(Lobby lobby)
        {
            // CRITICAL: Prevent the Host from also trying to start a client 
            // when they join the lobby they just created.
            if (lobby.HostId == AuthenticationService.Instance.PlayerId) return;

            Debug.Log("LobbyToNetwork (Local): Lobby joined, starting NGO as Client.");

            // 1. Package the Client's UGS Player ID to send to the Host
            string myUgsId = AuthenticationService.Instance.PlayerId;
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(myUgsId);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;

            // 2. Start the Client locally (defaults to looking for 127.0.0.1)
            NetworkManager.Singleton.StartClient();
        }

        private void HandleLeftLobby()
        {
            Debug.Log("LobbyToNetwork (Local): Left lobby, shutting down NGO.");
            NetworkManager.Singleton.Shutdown();
        }

        private void HandlePlayerKicked(string kickedUgsId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (ugsToNetcodeMap.TryGetValue(kickedUgsId, out ulong netcodeClientId))
                {
                    Debug.Log($"LobbyToNetwork (Local): Disconnecting Netcode Client ID {netcodeClientId}");
                    NetworkManager.Singleton.DisconnectClient(netcodeClientId);
                    ugsToNetcodeMap.Remove(kickedUgsId);
                }
            }
        }

        // --- HOST ONLY: Reads incoming Client data ---
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            byte[] payload = request.Payload;

            if (payload != null && payload.Length > 0)
            {
                string clientUgsId = System.Text.Encoding.UTF8.GetString(payload);

                // Map the UGS ID to the assigned Netcode Client ID
                ugsToNetcodeMap[clientUgsId] = request.ClientNetworkId;
                Debug.Log($"Mapped UGS ID {clientUgsId} to Netcode ID {request.ClientNetworkId}");
            }

            response.Approved = true;

            // False because your SpawnManager is going to handle spawning the physical player object!
            response.CreatePlayerObject = false;
        }
    }
}