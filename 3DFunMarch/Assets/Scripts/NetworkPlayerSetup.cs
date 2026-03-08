using Unity.Netcode;
using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach to the Player Prefab.
    /// Holt alle lokalen Komponenten automatisch per GetComponent –
    /// kein manuelles Zuweisen im Inspector nötig.
    /// </summary>
    public class NetworkPlayerSetup : NetworkBehaviour
    {
        [Header("Kamera (manuell zuweisen)")]
        [Tooltip("Das Kamera-GameObject des Spielers (Child-Objekt)")]
        public GameObject cameraObject;

        // Werden automatisch gefunden
        private FirstPersonController fpsController;
        private ObjectPickUp objectPickUp;
        private PlayerKnockback playerKnockback;
        private MonoBehaviour inputManager;

        private void Awake()
        {
            fpsController = GetComponent<FirstPersonController>();
            objectPickUp = GetComponent<ObjectPickUp>();
            playerKnockback = GetComponent<PlayerKnockback>();

            // InputManagerOld implementiert IInputManager – via MonoBehaviour holen
            inputManager = GetComponent<InputManagerOld>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
                SetupLocalPlayer();
            else
                SetupRemotePlayer();
        }

        private void SetupLocalPlayer()
        {
            SetLocalComponents(true);
            Debug.Log($"[NetworkPlayerSetup] Lokaler Spieler: {OwnerClientId}");
        }

        private void SetupRemotePlayer()
        {
            SetLocalComponents(false);

            // Kamera des Remote-Spielers komplett deaktivieren
            // (verhindert auch doppelten AudioListener)
            if (cameraObject != null)
                cameraObject.SetActive(false);

            Debug.Log($"[NetworkPlayerSetup] Remote-Spieler: {OwnerClientId}");
        }

        private void SetLocalComponents(bool enabled)
        {
            if (fpsController != null) fpsController.enabled = enabled;
            if (objectPickUp != null) objectPickUp.enabled = enabled;
            if (playerKnockback != null) playerKnockback.enabled = enabled;
            if (inputManager != null) inputManager.enabled = enabled;
        }
    }
}