using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Lobbies.Models;
using Unity.Netcode; 
using NetworkBaseNetwork;

namespace NetworkBaseRuntime
{
    [RequireComponent(typeof(UIDocument))]
    public class LobbyRoomUIManager : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _playerListContainer;
        private Button _leaveButton;
        private Button _startGameButton;
        [SerializeField] private string gameSceneName = "TheGame";
        [SerializeField] private string mainMenuSceneName = "StartMenu";
        private void OnEnable()
        {
            _document = GetComponent<UIDocument>();

            _playerListContainer = _document.rootVisualElement.Q<VisualElement>("PlayerListContainer");
            _leaveButton = _document.rootVisualElement.Q<Button>("Btn_LeaveLobby");
            _startGameButton = _document.rootVisualElement.Q<Button>("Btn_StartGame"); 

            if (_leaveButton != null)
            {
                _leaveButton.clicked += OnLeaveClicked;
            }

            if (_startGameButton != null)
            {
                _startGameButton.clicked += OnStartGameClicked;
            }
            if(LobbyServiceManager.Singleton.HostedLobby != null)
            {
                RefreshPlayerList(LobbyServiceManager.Singleton.HostedLobby);
            }
            else if(LobbyServiceManager.Singleton.JoinedLobby != null)
            {
                RefreshPlayerList(LobbyServiceManager.Singleton.JoinedLobby);
            }

            if(LobbyServiceManager.Singleton.JoinedLobby.HostId == LobbyServiceManager.Singleton.LocalPlayerId)
            {
                _startGameButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _startGameButton.style.display = DisplayStyle.None;
            }


            LobbyServiceManager.OnLobbyUpdated += RefreshPlayerList;
            LobbyServiceManager.OnLeftLobby += HandleLeftLobby;
        }

        private void OnDisable()
        {
            if (_leaveButton != null)
            {
                _leaveButton.clicked -= OnLeaveClicked;
            }

            if (_startGameButton != null)
            {
                _startGameButton.clicked -= OnStartGameClicked;
            }

            LobbyServiceManager.OnLobbyUpdated -= RefreshPlayerList;
            LobbyServiceManager.OnLeftLobby -= HandleLeftLobby;
        }

        private void RefreshPlayerList(Lobby currentLobby)
        {
            if (_playerListContainer == null) return;

            _playerListContainer.Clear();

            bool iAmHost = LobbyServiceManager.Singleton.HostedLobby != null;
            string localId = LobbyServiceManager.Singleton.LocalPlayerId;

            if (_startGameButton != null)
            {
                _startGameButton.style.display = iAmHost ? DisplayStyle.Flex : DisplayStyle.None;
            }

            // Loop through all players in the lobby data
            foreach (Player player in currentLobby.Players)
            {
                VisualElement playerRow = new VisualElement();
                playerRow.style.flexDirection = FlexDirection.Row;
                playerRow.style.justifyContent = Justify.SpaceBetween;
                playerRow.style.paddingBottom = 5;

                Label nameLabel = new Label();
                nameLabel.text = player.Id == localId ? "Player (Me)" : $"Player ({player.Id.Substring(0, 5)})";
                playerRow.Add(nameLabel);

                if (iAmHost && player.Id != localId)
                {
                    Button kickButton = new Button();
                    kickButton.text = "Kick";
                    kickButton.style.backgroundColor = new StyleColor(Color.red);

                    string playerToKick = player.Id;
                    kickButton.clicked += async () =>
                    {
                        Debug.Log($"UI====> Kicking player {playerToKick}");
                        await LobbyServiceManager.Singleton.KickPlayer(playerToKick);
                    };

                    playerRow.Add(kickButton);
                }

                _playerListContainer.Add(playerRow);
            }
        }

 
        private void OnStartGameClicked()
        {
            Debug.Log("UI====> Starting Game!");
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private async void OnLeaveClicked()
        {
            if (LobbyServiceManager.Singleton.HostedLobby != null)
            {
                await LobbyServiceManager.Singleton.DeleteLobby();
            }
            else
            {
                await LobbyServiceManager.Singleton.LeaveLobby();
            }
        }

        private void HandleLeftLobby()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}