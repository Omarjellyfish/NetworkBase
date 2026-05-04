using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace NetworkBaseRuntime
{
    public class MainMenuEvents : MonoBehaviour
    {
        UIDocument _document;
        private void OnEnable()
        {
            _document = GetComponent<UIDocument>();
            // Use the name exactly as it appears in your Hierarchy image
            var startButton = _document.rootVisualElement.Q<Button>("Btn_StartGame");

            if (startButton != null)
            {
                startButton.clicked += OnStartGame;
            }
        }

/*        private void OnDisable()
        {
            Debug.Log("MainMenuEvents OnDisable called, unsubscribing from events.");
            // Good practice to unsubscribe to avoid memory leaks
            var startButton = _document.rootVisualElement.Q<Button>("Btn_StartGame");
            if (startButton != null)
            {
                startButton.clicked -= OnStartGame;
            }
        }*/

        private void OnStartGame()
        {
            MyNetworkManager.Singleton.StartHost();
            Debug.Log("UI====>Start Host");
        }
    }
}
