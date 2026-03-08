using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach to the Player GameObject (neben FirstPersonController).
    /// Fängt Kollisionen mit dem Ball ab und versetzt den Spieler per
    /// CharacterController.Move() in einen Knockback-Zustand.
    /// Ignoriert Knockback wenn dieser Spieler den Ball selbst geworfen hat.
    /// </summary>
    public class PlayerKnockback : MonoBehaviour
    {
        [Header("Knockback Settings")]
        [Tooltip("Multiplikator auf die Aufprallgeschwindigkeit des Balls")]
        public float knockbackMultiplier = 2.5f;

        [Tooltip("Wie schnell der Knockback-Impuls abklingt (höher = schneller)")]
        public float knockbackDecay = 5f;

        [Tooltip("Wie stark der Spieler mit WASD dem Knockback entgegenwirken kann (0 = gar nicht)")]
        [Range(0f, 1f)]
        public float playerControlInfluence = 0.25f;

        [Tooltip("Minimale Ball-Geschwindigkeit damit ein Knockback ausgelöst wird")]
        public float minimumBallSpeed = 2f;

        private Vector3 knockbackVelocity = Vector3.zero;
        private FirstPersonController fpsController;
        private CharacterController characterController;

        private void Awake()
        {
            fpsController = GetComponent<FirstPersonController>();
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (knockbackVelocity.magnitude < 0.05f)
            {
                knockbackVelocity = Vector3.zero;
                return;
            }

            // Spieler-Input als leichte Gegenkraft
            Vector2 moveInput = fpsController.input.moveInput;
            Vector3 playerInfluence = transform.right   * moveInput.x * playerControlInfluence
                                    + transform.forward * moveInput.y * playerControlInfluence;

            characterController.Move((knockbackVelocity + playerInfluence) * Time.deltaTime);

            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!hit.collider.CompareTag("Ball")) return;

            Rigidbody ballRb = hit.collider.attachedRigidbody;
            if (ballRb == null) return;

            if (ballRb.linearVelocity.magnitude < minimumBallSpeed) return;

            // Werfer-Check: dieser Spieler hat den Ball geworfen → kein Knockback
            BallThrower thrower = hit.collider.GetComponent<BallThrower>();
            if (thrower != null && thrower.LastThrower == gameObject) return;

            Vector3 knockbackDir = hit.moveDirection;
            knockbackDir.y = 0.3f;
            knockbackDir.Normalize();

            knockbackVelocity = knockbackDir * ballRb.linearVelocity.magnitude * knockbackMultiplier;
        }
    }
}
