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
            Debug.Log("LobbyToNetwork (Runtime): Lobby created, starting NGO as Host.");


            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

            ugsToNetcodeMap.Clear();

            NetworkManager.Singleton.StartHost();
        }

        private void HandleLobbyJoined(Lobby lobby)
        {
            Debug.Log("LobbyToNetwork (Runtime): Lobby joined, starting NGO as Client.");

            // 2. Package the Client's UGS Player ID to send to the Host
            string myUgsId = AuthenticationService.Instance.PlayerId;
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(myUgsId);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;

            NetworkManager.Singleton.StartClient();
        }

        private void HandleLeftLobby()
        {
            Debug.Log("LobbyToNetwork (Runtime): Left lobby, shutting down NGO.");


            NetworkManager.Singleton.Shutdown();
        }

        private void HandlePlayerKicked(string kickedUgsId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (ugsToNetcodeMap.TryGetValue(kickedUgsId, out ulong netcodeClientId))
                {
                    Debug.Log($"LobbyToNetwork (Runtime): Disconnecting Netcode Client ID {netcodeClientId}");
                    NetworkManager.Singleton.DisconnectClient(netcodeClientId);

                    ugsToNetcodeMap.Remove(kickedUgsId);
                }
                else
                {
                    Debug.LogWarning("Tried to kick a player from Netcode, but their ID wasn't in the map!");
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
            response.CreatePlayerObject = false; // False for manual spawning
        }
    }
}