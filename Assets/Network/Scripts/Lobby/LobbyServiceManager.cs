using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace NetworkBaseNetwork
{
    public class LobbyServiceManager : MonoBehaviour
    {
        public static LobbyServiceManager Singleton { get; private set; }
        [Header("Lobby State")]
        public Lobby HostedLobby { get; private set; }
        public Lobby JoinedLobby { get; private set; }
        public string LocalPlayerId => AuthenticationService.Instance.PlayerId;

        [Header("Timers")]
        private float heartbeatTimer;
        private readonly float heartbeatInterval = 15f;
        private float lobbyPollTimer;
        private readonly float lobbyPollInterval = 1.1f;

        // --- EVENTS LISTEN TO ---
        public static event Action<Lobby> OnLobbyCreated;
        public static event Action<Lobby> OnLobbyJoined;
        public static event Action<List<Lobby>> OnLobbyListUpdated;
        public static event Action<Lobby> OnLobbyUpdated; 
        public static event Action OnLeftLobby;
        public static event Action<string> OnPlayerKicked;

        private bool isInitialized = false;
        private void Awake() 
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await InitializeUnityServices();

            isInitialized = true;
        }

        private async Task InitializeUnityServices()
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in! PlayerID: {AuthenticationService.Instance.PlayerId}");
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void Update()
        {
            if (!isInitialized) return;

            HandleLobbyHeartbeat();
            HandleLobbyPolling();
        }

        private async void HandleLobbyHeartbeat()
        {
            if (HostedLobby != null)
            {
                heartbeatTimer += Time.deltaTime;
                if (heartbeatTimer >= heartbeatInterval)
                {
                    heartbeatTimer = 0f;
                    await LobbyService.Instance.SendHeartbeatPingAsync(HostedLobby.Id);
                }
            }
        }

        private async void HandleLobbyPolling()
        {
            if (JoinedLobby == null || string.IsNullOrEmpty(JoinedLobby.Id)) return;

            lobbyPollTimer += Time.deltaTime;
            if (lobbyPollTimer < lobbyPollInterval) return;

            lobbyPollTimer = 0f;

            // 1. Capture the ID locally so it can't change mid-execution
            string lobbyIdToPoll = JoinedLobby.Id;

            try
            {
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(lobbyIdToPoll);

                // 2. CRITICAL: Check if we are STILL in a lobby after the await.
                // If the user clicked "Leave" while the network was thinking, JoinedLobby is now null.
                if (JoinedLobby == null) return;

                // make sure I'm still in the lobby list returned by the server
                bool isPlayerStillInLobby = false;
                if (lobby.Players != null)
                {
                    foreach (Player player in lobby.Players)
                    {
                        if (player.Id == AuthenticationService.Instance.PlayerId)
                        {
                            isPlayerStillInLobby = true;
                            break;
                        }
                    }
                }

                if (!isPlayerStillInLobby)
                {
                    Debug.Log("I have been kicked or lobby closed!");
                    JoinedLobby = null;
                    HostedLobby = null; // Clear host ref too if applicable
                    OnLeftLobby?.Invoke();
                    return;
                }

                JoinedLobby = lobby;
                OnLobbyUpdated?.Invoke(JoinedLobby);
            }
            catch (LobbyServiceException e)
            {
                // If the lobby is not found (404), it means it was deleted
                if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    JoinedLobby = null;
                    OnLeftLobby?.Invoke();
                }
                Debug.LogWarning($"Lobby Service Error: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Polling Error: {e.Message}");
            }
        }
        public void StopPolling()
        {
            JoinedLobby = null;
        }

        public async Task CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions { IsPrivate = isPrivate };
                HostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                JoinedLobby = HostedLobby;

                Debug.Log($"Created Lobby: {HostedLobby.Name} with Code: {HostedLobby.LobbyCode}");
                OnLobbyCreated?.Invoke(HostedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to create lobby: {e.Message}");
            }
        }

        public async Task ListLobbies()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    },
                    Order = new List<QueryOrder> { new QueryOrder(false, QueryOrder.FieldOptions.Created) }
                };

                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
                OnLobbyListUpdated?.Invoke(response.Results); // Pass data to UI
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to list lobbies: {e.Message}");
            }
        }

        public async Task JoinLobbyById(string lobbyId)
        {
            try
            {
                JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
                OnLobbyJoined?.Invoke(JoinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to join lobby: {e.Message}");
            }
        }

        public async Task KickPlayer(string playerId)
        {
            if (HostedLobby != null)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(HostedLobby.Id, playerId);
                    OnPlayerKicked?.Invoke(playerId);
                    Debug.Log("Player kicked successfully.");
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Failed to kick player: {e.Message}");
                }
            }
        }

        public async Task LeaveLobby()
        {
            if (JoinedLobby != null)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
                    HostedLobby = null;
                    JoinedLobby = null;
                    OnLeftLobby?.Invoke();
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Failed to leave lobby: {e.Message}");
                }
            }
        }
        public async Task QuickJoinLobby()
        {
            try
            {
                JoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                Debug.Log($"Successfully quick joined Lobby: {JoinedLobby.Name} with Code: {JoinedLobby.LobbyCode}");
                OnLobbyJoined?.Invoke(JoinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to quick join lobby: {e.Message}");
            }
        }
        public async Task JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                JoinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
                Debug.Log($"Successfully joined Lobby with code: {lobbyCode}");
                OnLobbyJoined?.Invoke(JoinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to join lobby by code: {e.Message}");
            }
        }

        public async Task DeleteLobby()
        {
            if (HostedLobby != null)
            {
                try
                {
                    await LobbyService.Instance.DeleteLobbyAsync(HostedLobby.Id);
                    Debug.Log("Lobby deleted successfully.");
                    HostedLobby = null;
                    JoinedLobby = null;

                    OnLeftLobby?.Invoke();
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Failed to delete lobby: {e.Message}");
                    Debug.LogError($"Failed to delete lobby: {e.Reason}");
                }
            }
        }
    }
}