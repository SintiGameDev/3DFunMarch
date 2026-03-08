using Unity.Netcode;
using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Einfaches OnGUI-Menü zum Starten als Host oder Client.
    /// Attach to any GameObject in the scene (z.B. NetworkManager).
    /// Verschwindet sobald eine Verbindung hergestellt wurde.
    /// </summary>
    public class NetworkLobbyUI : MonoBehaviour
    {
        [Header("Verbindung")]
        [Tooltip("IP-Adresse des Hosts (nur für Client relevant)")]
        public string hostAddress = "127.0.0.1";

        private bool isConnected = false;
        private string statusMessage = "";

        private void OnGUI()
        {
            if (isConnected) return;

            // Zentriertes Box-Layout
            float boxWidth  = 300f;
            float boxHeight = 180f;
            float x = (Screen.width  - boxWidth)  / 2f;
            float y = (Screen.height - boxHeight) / 2f;

            GUILayout.BeginArea(new Rect(x, y, boxWidth, boxHeight), GUI.skin.box);

            GUILayout.Label("🏐 Völkerball – Lobby", GUI.skin.label);
            GUILayout.Space(10);

            GUILayout.Label("Host IP:");
            hostAddress = GUILayout.TextField(hostAddress);
            GUILayout.Space(8);

            if (GUILayout.Button("Als Host starten"))
                StartHost();

            if (GUILayout.Button("Als Client joinen"))
                StartClient();

            if (!string.IsNullOrEmpty(statusMessage))
                GUILayout.Label(statusMessage);

            GUILayout.EndArea();
        }

        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            isConnected = true;
            statusMessage = "Host gestartet.";
            Debug.Log("[NetworkLobbyUI] Host gestartet.");
        }

        private void StartClient()
        {
            // IP setzen bevor Connect
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
                transport.SetConnectionData(hostAddress, 7777);

            NetworkManager.Singleton.StartClient();
            isConnected = true;
            statusMessage = $"Verbinde mit {hostAddress}...";
            Debug.Log($"[NetworkLobbyUI] Client verbindet mit {hostAddress}.");
        }
    }
}
