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
            var startButton = _document.rootVisualElement.Q<Button>("Btn_StartGame");

            if (startButton != null)
            {
                startButton.clicked += OnStartGame;
            }
        }

/*        private void OnDisable()
        {
            Debug.Log("MainMenuEvents OnDisable called, unsubscribing from events.");
            
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
