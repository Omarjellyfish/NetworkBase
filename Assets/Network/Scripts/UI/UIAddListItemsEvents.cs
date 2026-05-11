using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace NetworkBaseNetwork
{
    public class UIAddListItemsEvents : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

       

        private ListView lobbyListView;
        private Button backButton;

        private void OnEnable()
        {
            var root = uiDocument.rootVisualElement;

            lobbyListView = root.Q<ListView>("LobbyList");
            
            // Fix NaN error: ListView needs a fixed item height to calculate scroll boundaries properly!
            lobbyListView.fixedItemHeight = 50f;

            lobbyListView.makeItem = () =>
            {
                Label generatedLabel = new Label();

                generatedLabel.style.fontSize = 20;
                generatedLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                generatedLabel.style.paddingLeft = 10;
                generatedLabel.style.paddingTop = 5;
                generatedLabel.style.paddingBottom = 5;

                return generatedLabel;
            };

            lobbyListView.bindItem = (VisualElement element, int index) =>
            {
                Label nameLabel = element as Label;

                var sourceList = lobbyListView.itemsSource as List<Lobby>;
                if (sourceList == null || index >= sourceList.Count) return;

                Lobby currentLobby = sourceList[index];
                if (currentLobby == null) return;

                int currentPlayers = currentLobby.MaxPlayers - currentLobby.AvailableSlots;
                string lobbyName = string.IsNullOrEmpty(currentLobby.Name) ? "Unnamed Lobby" : currentLobby.Name;

                nameLabel.text = $"{lobbyName} ({currentPlayers}/{currentLobby.MaxPlayers})";
            };

            backButton = root.Q<Button>("BackButton");
            if (backButton != null)
            {
                backButton.clicked += OnBackClicked;
            }
        }

        private void OnDisable()
        {
            if (backButton != null)
            {
                backButton.clicked -= OnBackClicked;
            }
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
        }

        public void PopulateAndShow(List<Lobby> lobbies)
        {
            gameObject.SetActive(true);
            lobbyListView.itemsSource = lobbies;
            lobbyListView.Rebuild();
        }
    }
}