using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach to the Ball GameObject.
    /// Kein SetParent – NGO erlaubt kein Parenting unter Nicht-NetworkObjects.
    /// Stattdessen: Position wird jeden Frame manuell auf den GrabPoint gesetzt,
    /// während der NetworkTransform vorübergehend deaktiviert wird.
    /// </summary>
    public class NetworkBallSync : NetworkBehaviour
    {
        // Speichert die NetworkObjectId des Spielers der den Ball hält (0 = niemand)
        private NetworkVariable<ulong> holderNetworkObjectId = new NetworkVariable<ulong>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private Rigidbody rb;
        private NetworkTransform networkTransform;

        // Lokale Referenz auf den GrabPoint – nur auf dem haltenden Client gesetzt
        private Transform currentGrabPoint;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            networkTransform = GetComponent<NetworkTransform>();
        }

        public override void OnNetworkSpawn()
        {
            holderNetworkObjectId.OnValueChanged += OnHolderChanged;
        }

        public override void OnNetworkDespawn()
        {
            holderNetworkObjectId.OnValueChanged -= OnHolderChanged;
        }

        // ─── Pickup ────────────────────────────────────────────────────────────

        public void RequestPickup(Transform grabPoint, GameObject pickerObject)
        {
            currentGrabPoint = grabPoint;

            var networkObject = pickerObject.GetComponent<NetworkObject>();
            if (networkObject == null) return;

            PickupServerRpc(networkObject.NetworkObjectId);
        }

        [Rpc(SendTo.Server)]
        private void PickupServerRpc(ulong pickerNetworkObjectId)
        {
            holderNetworkObjectId.Value = pickerNetworkObjectId;
        }

        // ─── Throw ─────────────────────────────────────────────────────────────

        public void RequestThrow(Vector3 force)
        {
            currentGrabPoint = null;
            ThrowServerRpc(force);
        }

        [Rpc(SendTo.Server)]
        private void ThrowServerRpc(Vector3 force)
        {
            holderNetworkObjectId.Value = 0;

            // NetworkTransform wieder aktivieren – ab jetzt synchronisiert NGO die Position
            if (networkTransform != null)
                networkTransform.enabled = true;

            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
        }

        // ─── Zustandsänderung (alle Clients) ───────────────────────────────────

        private void OnHolderChanged(ulong previous, ulong current)
        {
            if (current == 0)
            {
                // Ball losgelassen – Physik und NetworkTransform freigeben
                rb.isKinematic = false;
                if (networkTransform != null)
                    networkTransform.enabled = true;
            }
            else
            {
                // Ball wird gehalten – Physik & NetworkTransform einfrieren
                // NetworkTransform deaktivieren damit wir die Position manuell setzen können
                rb.isKinematic = true;
                if (networkTransform != null)
                    networkTransform.enabled = false;
            }
        }

        private void Update()
        {
            // Nur auf dem Client der den Ball hält: Position manuell auf GrabPoint setzen
            // Kein SetParent – stattdessen Weltposition direkt übernehmen
            if (currentGrabPoint != null && holderNetworkObjectId.Value != 0)
            {
                transform.position = currentGrabPoint.position;
                transform.rotation = currentGrabPoint.rotation;
            }
        }
    }
}
