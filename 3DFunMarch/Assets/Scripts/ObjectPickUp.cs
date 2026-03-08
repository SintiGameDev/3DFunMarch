using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach this script to the Player GameObject.
    /// Requires: FirstPersonController on the same GameObject.
    /// Ball requires: Rigidbody + SphereCollider, Tag = "Ball"
    /// </summary>
    public class ObjectPickUp : MonoBehaviour
    {
        [Header("Pickup Settings")]
        public float pickupRange = 3f;
        public float throwForce = 15f;
        public LayerMask ballLayer;

        [Header("Hold Position")]
        [Tooltip("Offset vom Camera-Transform: z.B. (0, -0.2, 0.8)")]
        public Vector3 holdOffset = new Vector3(0f, -0.2f, 0.8f);

        // Interner Zustand
        private Rigidbody heldBall;
        private FirstPersonController fpsController;

        private void Awake()
        {
            fpsController = GetComponent<FirstPersonController>();
        }

        private void Update()
        {
            // LMT gedrückt → Pickup versuchen (nur wenn kein Ball gehalten wird)
            if (Input.GetMouseButtonDown(0) && heldBall == null)
            {
                TryPickup();
            }

            // LMT losgelassen → Ball werfen
            if (Input.GetMouseButtonUp(0) && heldBall != null)
            {
                ThrowBall();
            }

            // Ball zur Halteposition mitführen
            if (heldBall != null)
            {
                CarryBall();
            }
        }

        private void TryPickup()
        {
            Transform cam = fpsController.playerCamera;
            Ray ray = new Ray(cam.position, cam.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, ballLayer))
            {
                Rigidbody rb = hit.rigidbody;
                if (rb != null)
                {
                    heldBall = rb;
                    heldBall.isKinematic = true;
                    heldBall.transform.SetParent(cam);
                    heldBall.transform.localPosition = holdOffset;
                    heldBall.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private void CarryBall()
        {
            // Position wird über SetParent + localPosition gehalten.
            // Alternativ: Smoothdamp für weichere Bewegung (optional).
            heldBall.transform.localPosition = holdOffset;
        }

        private void ThrowBall()
        {
            Transform cam = fpsController.playerCamera;

            heldBall.transform.SetParent(null);
            heldBall.isKinematic = false;
            heldBall.linearVelocity = Vector3.zero;
            heldBall.AddForce(cam.forward * throwForce, ForceMode.Impulse);

            heldBall = null;
        }
    }
}