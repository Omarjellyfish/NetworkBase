using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using NetworkBaseNetwork;
using System;

namespace NetworkBaseRuntime
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuUIManager : MonoBehaviour
    {
        private UIDocument _document;

        // UI Elements
        private Button _btnHost;
        private Button _btnRefresh; 
        private VisualElement _lobbyListContainer;

        // Overlay and Join Code Elements
        private Button _btnOpenBrowser;
        private Button _btnCloseBrowser;
        private Button _btnQuickJoin;
        private VisualElement _serverBrowserOverlay;
        private TextField _joinCodeInput;
        private Button _btnJoinByCode;

        private void OnEnable()
        {

            // Get references to UI elements
            _document = GetComponent<UIDocument>();

            _btnHost = _document.rootVisualElement.Q<Button>("Btn_HostGame");
            _btnRefresh = _document.rootVisualElement.Q<Button>("Btn_RefreshLobbies");
            _lobbyListContainer = _document.rootVisualElement.Q<VisualElement>("LobbyListContainer");

            _btnOpenBrowser = _document.rootVisualElement.Q<Button>("Btn_OpenBrowser");
            _btnCloseBrowser = _document.rootVisualElement.Q<Button>("Btn_CloseBrowser");
            _serverBrowserOverlay = _document.rootVisualElement.Q<VisualElement>("ServerBrowserOverlay");

            _joinCodeInput = _document.rootVisualElement.Q<TextField>("Input_JoinCode");
            _btnJoinByCode = _document.rootVisualElement.Q<Button>("Btn_JoinByCode");
            _btnQuickJoin=_document.rootVisualElement.Q<Button>("Btn_QuickJoin");



            // event subscriptions
            if (_serverBrowserOverlay != null)
            {
                _serverBrowserOverlay.style.display = DisplayStyle.None;
            }

            if (_btnHost != null) _btnHost.clicked += OnHostClicked;
            if (_btnRefresh != null) _btnRefresh.clicked += OnRefreshClicked;

            if (_btnOpenBrowser != null) _btnOpenBrowser.clicked += OpenBrowserOverlay;
            if (_btnCloseBrowser != null) _btnCloseBrowser.clicked += CloseBrowserOverlay;
            if (_btnJoinByCode != null) _btnJoinByCode.clicked += OnJoinByCodeClicked;
            if (_btnQuickJoin != null) _btnQuickJoin.clicked += OnQuickJoinClicked;

            LobbyServiceManager.OnLobbyListUpdated += DrawLobbyList;
        }



        private void OnDisable()
        {
            if (_btnHost != null) _btnHost.clicked -= OnHostClicked;
            if (_btnRefresh != null) _btnRefresh.clicked -= OnRefreshClicked;

            if (_btnOpenBrowser != null) _btnOpenBrowser.clicked -= OpenBrowserOverlay;
            if (_btnCloseBrowser != null) _btnCloseBrowser.clicked -= CloseBrowserOverlay;
            if (_btnJoinByCode != null) _btnJoinByCode.clicked -= OnJoinByCodeClicked;

            LobbyServiceManager.OnLobbyListUpdated -= DrawLobbyList;
        }

        private async void OnHostClicked()
        {
            Debug.Log("UI====> Requesting to Host a Lobby...");
            _btnHost.SetEnabled(false);
            await LobbyServiceManager.Singleton.CreateLobby("New Awesome Game", 4, false);
            _btnHost.SetEnabled(true);
        }

        private async void OnRefreshClicked()
        {
            Debug.Log("UI====> Requesting Lobby List...");
            await LobbyServiceManager.Singleton.ListLobbies();
        }

        private async void OpenBrowserOverlay()
        {
            if (_serverBrowserOverlay != null)
            {
                _serverBrowserOverlay.style.display = DisplayStyle.Flex; 
                await LobbyServiceManager.Singleton.ListLobbies(); 
            }
        }

        private void CloseBrowserOverlay()
        {
            if (_serverBrowserOverlay != null)
            {
                _serverBrowserOverlay.style.display = DisplayStyle.None; 
            }
        }

  
        private async void OnJoinByCodeClicked()
        {
            if (_joinCodeInput != null && !string.IsNullOrEmpty(_joinCodeInput.value))
            {
                Debug.Log($"UI====> Attempting to join with code: {_joinCodeInput.value}");
                _btnJoinByCode.SetEnabled(false);

                await LobbyServiceManager.Singleton.JoinLobbyByCode(_joinCodeInput.value);

                _btnJoinByCode.SetEnabled(true);
            }
        }

        private void OnQuickJoinClicked()
        {
            if(_btnQuickJoin != null)
            {
                _btnQuickJoin.SetEnabled(false);
                Debug.Log("UI====> Attempting Quick Join...");
                LobbyServiceManager.Singleton.QuickJoinLobby().ContinueWith(_ =>
                {
                    _btnQuickJoin.SetEnabled(true);
                });
            }
        }

        private void DrawLobbyList(List<Lobby> lobbies)
        {
            if (_lobbyListContainer == null) return;

            _lobbyListContainer.Clear();

            if (lobbies.Count == 0)
            {
                _lobbyListContainer.Add(new Label("No lobbies found."));
                return;
            }

            foreach (Lobby lobby in lobbies)
            {
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.paddingBottom = 10;

                Label nameLabel = new Label($"{lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})");
                row.Add(nameLabel);

                Button joinButton = new Button();
                joinButton.text = "Join";

                string lobbyIdToJoin = lobby.Id;
                joinButton.clicked += async () =>
                {
                    Debug.Log($"UI====> Joining Lobby: {lobby.Name}");
                    await LobbyServiceManager.Singleton.JoinLobbyById(lobbyIdToJoin);
                };

                row.Add(joinButton);
                _lobbyListContainer.Add(row);
            }
        }
    }
}