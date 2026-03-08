using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach to the Ball GameObject.
    /// Speichert wer den Ball zuletzt geworfen hat, damit PlayerKnockback
    /// den Werfer selbst vom Knockback ausschließen kann.
    /// </summary>
    public class BallThrower : MonoBehaviour
    {
        public GameObject LastThrower { get; private set; }

        public void SetThrower(GameObject thrower)
        {
            LastThrower = thrower;
        }

        public void ClearThrower()
        {
            LastThrower = null;
        }
    }
}
