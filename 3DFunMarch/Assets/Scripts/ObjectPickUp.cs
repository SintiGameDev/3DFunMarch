using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    /// <summary>
    /// Attach to the Player GameObject.
    /// Ball-Positionierung wird an NetworkBallSync delegiert – kein SetParent mehr.
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

        private Rigidbody heldBall;
        private BallThrower heldBallThrower;
        private NetworkBallSync heldBallSync;
        private FirstPersonController fpsController;

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
                if (rb == null) return;

                ClearHighlight();

                heldBall        = rb;
                heldBallThrower = rb.GetComponent<BallThrower>();
                heldBallSync    = rb.GetComponent<NetworkBallSync>();

                // Physik lokal einfrieren
                heldBall.isKinematic = true;

                // Netzwerk: Pickup melden – NetworkBallSync übernimmt die Positionierung
                if (heldBallSync != null)
                    heldBallSync.RequestPickup(grabPoint, gameObject);

                if (heldBallThrower != null)
                    heldBallThrower.SetThrower(gameObject);
            }
        }

        private void ThrowBall()
        {
            Transform cam = fpsController.playerCamera;
            Vector3 force = cam.forward * throwForce;

            // Lokal freigeben
            heldBall.isKinematic = false;
            heldBall.linearVelocity = Vector3.zero;
            heldBall.AddForce(force, ForceMode.Impulse);

            // Netzwerk: Wurf melden
            if (heldBallSync != null)
                heldBallSync.RequestThrow(force);

            if (heldBallThrower != null)
                heldBallThrower.SetThrower(gameObject);

            heldBall        = null;
            heldBallThrower = null;
            heldBallSync    = null;
        }
    }
}
