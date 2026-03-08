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
        [Tooltip("Child-Transform der Kamera – definiert wo der Ball gehalten wird")]
        public Transform grabPoint;

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
                    heldBall.transform.SetParent(grabPoint);
                    heldBall.transform.localPosition = Vector3.zero;
                    heldBall.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private void CarryBall()
        {
            heldBall.transform.localPosition = Vector3.zero;
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