using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Dieses Script auf ein beliebiges Trigger-Objekt legen (z.B. unsichtbare
    /// Box unterhalb der Map). Sobald ein Objekt mit Tag "Player" den Trigger
    /// betritt, informiert es den GameManager.
    ///
    /// Setup:
    ///  - Collider der Zone: "Is Trigger" = true
    ///  - Dieses Script als Komponente hinzufügen
    /// </summary>
    public class OutOfBoundsTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerOutOfBounds(other.gameObject);
            else
                Debug.LogWarning("[OutOfBoundsTrigger] Kein GameManager in der Szene gefunden!");
        }
    }
}
