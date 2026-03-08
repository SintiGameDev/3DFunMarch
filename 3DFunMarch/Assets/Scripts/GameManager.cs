using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Singleton GameManager – verwaltet den Spielzustand.
    /// Aktuell: Szenen-Reset wenn ein Spieler eine Out-of-Bounds-Zone betritt.
    /// Erweiterbar um: Teams, Punktestand, Rundensystem.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Debug")]
        public bool logEvents = true;

        private void Awake()
        {
            // Singleton – nur eine Instanz erlaubt
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Wird von OutOfBoundsTrigger aufgerufen wenn ein Spieler die Zone betritt.
        /// </summary>
        public void OnPlayerOutOfBounds(GameObject player)
        {
            if (logEvents)
                Debug.Log($"[GameManager] {player.name} ist aus dem Spielfeld gefallen – Szene wird neu geladen.");

            ReloadScene();
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
