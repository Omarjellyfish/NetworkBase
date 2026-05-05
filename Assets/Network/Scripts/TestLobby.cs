using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetworkBaseNetwork
{
    public class TestLobby : MonoBehaviour
    {
        private Lobby hostedLobby;
        private float heartbeatTimer;
        private readonly float heartbeatInterval = 15f;
        [SerializeField] private string lobbyCode;

        [SerializeField] private UIAddListItemsEvents lobbyListUI;

        private async void Start()
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in! PlayerID: " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void Update()
        {
            HandleLobbyHeartbeat();
        }

        private async void HandleLobbyHeartbeat()
        {
            if (hostedLobby != null)
            {
                heartbeatTimer += Time.deltaTime;
                if (heartbeatTimer >= heartbeatInterval)
                {
                    heartbeatTimer = 0f;
                    try
                    {
                        await LobbyService.Instance.SendHeartbeatPingAsync(hostedLobby.Id);
                    }
                    catch (LobbyServiceException e)
                    {
                        Debug.LogError("Failed to send heartbeat: " + e.Message);
                    }
                }
            }
        }

        public async void OnSearchAndOpenLobbyMenu()
        {
            Debug.Log("Searching for lobbies...");

            // 1. Fetch the data
            List<Lobby> foundLobbies = await ListLobbies();

            // 2. Pass the data to the UI script and display it
            if (lobbyListUI != null)
            {
                lobbyListUI.PopulateAndShow(foundLobbies);
            }
            else
            {
                Debug.LogError("LobbyListUI is not assigned in the Inspector!");
            }
        }

        // --- DEBUG FUNCTIONS (Right-click script in Inspector to use) ---

        [ContextMenu("Debug: Create Test Lobby")]
        public void DebugCreateLobby()
        {
            // Creates a public lobby named "Debug Game" with 4 players
            CreateLobby(false, 4, "Debug Game");
        }

        [ContextMenu("Debug: Test List Lobbies")]
        public async void DebugListLobbies()
        {
            await ListLobbies();
        }

        // --- EXISTING LOGIC ---

        public async Task<List<Lobby>> ListLobbies()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 20,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    },
                    Order = new List<QueryOrder>
                    {
                        new QueryOrder(false, QueryOrder.FieldOptions.Created)
                    }
                };

                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
                Debug.Log("Lobbies found: " + response.Results.Count);
                return response.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                return new List<Lobby>();
            }
        }

        public async void CreateLobby(bool isPrivate, int MaxPlayerCount, string lobbyName)
        {
            if (hostedLobby != null)
            {
                Debug.LogWarning("You are already hosting a lobby!");
                return;
            }

            try
            {
                CreateLobbyOptions clo = new CreateLobbyOptions { IsPrivate = isPrivate };
                hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MaxPlayerCount, clo);

                Debug.Log("Successfully created Lobby: " + hostedLobby.Name + " Lobby Code: " + hostedLobby.LobbyCode);
                lobbyCode = hostedLobby.LobbyCode;
                heartbeatTimer = 0f;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async Task QuickJoinLobby()
        {
            try
            {
                hostedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                Debug.Log("Successfully joined Lobby: " + hostedLobby.Name);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void JoinLobbyByCode(string code)
        {
            try
            {
                hostedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
                Debug.Log("Successfully joined Lobby: " + hostedLobby.Name);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void DeleteLobby()
        {
            if (hostedLobby != null)
            {
                try
                {
                    await LobbyService.Instance.DeleteLobbyAsync(hostedLobby.Id);
                    Debug.Log("Lobby deleted successfully.");
                    hostedLobby = null;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }
        }
    }
}