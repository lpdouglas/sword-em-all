using TMPro;
using UnityEngine;
using Mirror;
using System.Collections;


namespace TGM.Multiplayer
{
    public class ScreenGui : MonoBehaviour
    {
        static ScreenGui instance;
        public TextMeshProUGUI _matchText;
        public GameObject connectScreen;
        public GameObject startMatchScreen;
        public GameObject waitingHostStartScreen;
        public TextMeshProUGUI infoText;

        public static TextMeshProUGUI matchText;

        void Awake()
        {
            instance = this;
            matchText = instance._matchText;
            infoText.text = "";

            connectScreen.SetActive(true);
            startMatchScreen.SetActive(false);
            waitingHostStartScreen.SetActive(false);
        }

        public void JoinMatch()
        {
            NetworkManager.singleton.StartClient();
        }

        public void CreateServer()
        {
            NetworkManager.singleton.StartServer();
        }

        public void CreateServerHost()
        {
            NetworkManager.singleton.StartHost();
        }

        public void StartMatch()
        {
            LobbyNetworkManager.instance.StartMatch();
        }

        void ShowInfoText(string text)
        {
            infoText.text = text;
            Invoke("UnshowInfoText", 3);
        }

        void UnshowInfoText() => infoText.text = "";

        internal static void ShowError(string text) => instance.ShowInfoText(text);

        public static void OnStartServer()
        {
            instance.ShowInfoText("Server is On");
            instance.connectScreen.SetActive(false);
            instance.startMatchScreen.SetActive(false);
            instance.waitingHostStartScreen.SetActive(false);
        }


        public static void OnClientConnect(float delay = 0) => instance._OnClientConnect(delay);
        public void _OnClientConnect(float delay)
        {
            Debug.Log("Conectou no servidor, is host? " + ConnectionPlayer.isHost);
            instance.ShowInfoText("Connected on Server");
            StartCoroutine(ShowMatchMenu(delay));
        }

        private IEnumerator ShowMatchMenu(float delay)
        {
            yield return new WaitForSeconds(delay);
            instance.connectScreen.SetActive(false);
            instance.startMatchScreen.SetActive(ConnectionPlayer.isHost);
            instance.waitingHostStartScreen.SetActive(!ConnectionPlayer.isHost);
        }

        public static void OnStartMatch()
        {
            instance.connectScreen.SetActive(false);
            instance.startMatchScreen.SetActive(false);
            instance.waitingHostStartScreen.SetActive(false);
        }

        internal static void OnStopClient()
        {
            instance.connectScreen.SetActive(true);
            instance.startMatchScreen.SetActive(false);
            instance.waitingHostStartScreen.SetActive(false);
        }
    }

}