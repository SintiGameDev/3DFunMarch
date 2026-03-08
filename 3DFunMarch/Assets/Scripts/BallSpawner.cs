using Unity.Netcode;
using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach to any GameObject in the scene (z.B. NetworkManager oder eigenes Spawner-Objekt).
    /// Spawnt den Ball automatisch wenn der Host/Server startet.
    /// Nur der Server darf NetworkObjects spawnen – Clients ignorieren OnNetworkSpawn.
    ///
    /// Setup:
    ///  - ballPrefab: Ball-Prefab aus dem Project-Fenster zuweisen
    ///  - spawnPoint: Optionaler Transform für die Spawn-Position (sonst Vector3.zero)
    ///  - Ball-Prefab muss in der NetworkPrefabs-Liste des NetworkManagers eingetragen sein
    /// </summary>
    public class BallSpawner : NetworkBehaviour
    {
        [Header("Ball Settings")]
        [Tooltip("Ball-Prefab aus dem Project-Fenster (muss NetworkObject haben)")]
        public GameObject ballPrefab;

        [Tooltip("Spawn-Position des Balls – leer lassen für Weltmittelpunkt")]
        public Transform spawnPoint;

        public override void OnNetworkSpawn()
        {
            // Nur der Server/Host spawnt – Clients tun nichts
            if (!IsServer) return;

            SpawnBall();
        }

        private void SpawnBall()
        {
            if (ballPrefab == null)
            {
                Debug.LogError("[BallSpawner] Kein Ball-Prefab zugewiesen!");
                return;
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero + Vector3.up;
            Quaternion rotation = Quaternion.identity;

            GameObject ball = Instantiate(ballPrefab, position, rotation);

            // Ball über das Netzwerk spawnen – alle Clients erhalten automatisch eine Instanz
            ball.GetComponent<NetworkObject>().Spawn();

            Debug.Log($"[BallSpawner] Ball gespawnt bei {position}");
        }
    }
}