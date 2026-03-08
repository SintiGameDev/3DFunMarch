using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach this script to the Player GameObject.
    /// Requires: FirstPersonController on the same GameObject.
    /// Ball requires: Rigidbody + SphereCollider + BallThrower, Layer = ballLayer
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

        [Header("Highlight")]
        [Tooltip("Material das angezeigt wird wenn der Spieler den Ball ansieht")]
        public Material highlightMaterial;

        // Interner Zustand
        private Rigidbody heldBall;
        private BallThrower heldBallThrower;
        private FirstPersonController fpsController;

        // Highlight-Tracking
        private Renderer currentHighlightedRenderer;
        private Material[] originalMaterials;

        private void Awake()
        {
            fpsController = GetComponent<FirstPersonController>();
        }

        private void Update()
        {
            if (heldBall == null)
            {
                UpdateHighlight();

                if (Input.GetMouseButtonDown(0))
                    TryPickup();
            }
            else
            {
                CarryBall();

                if (Input.GetMouseButtonUp(0))
                    ThrowBall();
            }
        }

        private void UpdateHighlight()
        {
            Transform cam = fpsController.playerCamera;
            Ray ray = new Ray(cam.position, cam.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, ballLayer))
            {
                Renderer hitRenderer = hit.collider.GetComponent<Renderer>();

                if (hitRenderer != null && hitRenderer != currentHighlightedRenderer)
                {
                    ClearHighlight();
                    ApplyHighlight(hitRenderer);
                }
            }
            else
            {
                ClearHighlight();
            }
        }

        private void ApplyHighlight(Renderer rend)
        {
            currentHighlightedRenderer = rend;
            originalMaterials = rend.materials;

            Material[] highlighted = new Material[originalMaterials.Length];
            for (int i = 0; i < highlighted.Length; i++)
                highlighted[i] = highlightMaterial;

            rend.materials = highlighted;
        }

        private void ClearHighlight()
        {
            if (currentHighlightedRenderer == null) return;

            currentHighlightedRenderer.materials = originalMaterials;
            currentHighlightedRenderer = null;
            originalMaterials = null;
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
                    ClearHighlight();

                    heldBall = rb;
                    heldBallThrower = rb.GetComponent<BallThrower>();

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

            // Werfer auf dem Ball registrieren
            if (heldBallThrower != null)
                heldBallThrower.SetThrower(gameObject);

            heldBall = null;
            heldBallThrower = null;
        }
    }
}
