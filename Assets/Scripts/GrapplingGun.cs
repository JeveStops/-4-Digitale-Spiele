using UnityEngine;
using TMPro;

public class GrapplingGun : MonoBehaviour
{
    [Header("GRappleGun Presets")]
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, playerCamera, player;
    private float maxDistance = 100f;
    private SpringJoint joint;

    private Transform grabbedTransform;
    private Vector3 grabbedOffset;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // Linksklick: Grapple starten
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        // Objekt (oder dich selbst) mit dem Mausrad heranziehen
        if (joint != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                joint.maxDistance -= scroll * 15f;
                if (joint.maxDistance < 1f) joint.maxDistance = 1f;
            }
        }
    }

    // Late Update, damit die Visualisierung korrekt erst nach allen Berechnungen ausgeführt wird
    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;

        // Wenn der RayCast etwas treffbares berührt wird eine Spring Joint erzeugt (Eine Feder zwischen Spieler und Objekt letztendlich)
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;

            if (hit.rigidbody != null)
            {
                joint.connectedBody = hit.rigidbody;
                joint.connectedAnchor = hit.transform.InverseTransformPoint(hit.point);
            }
            else
            {
                joint.connectedAnchor = grapplePoint;
            }

            grabbedTransform = hit.transform;
            grabbedOffset = hit.transform.InverseTransformPoint(hit.point);

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }

    // Los Lösung des Joints und resetten aller wichtigen Attribute des Greifhaken-Punktes
    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
        grabbedTransform = null;
    }

    private Vector3 currentGrapplePosition;

    // Visualisierung des Seils
    void DrawRope()
    {
        if (!joint) return;

        Vector3 realGrapplePoint = (grabbedTransform != null) ? grabbedTransform.TransformPoint(grabbedOffset) : grapplePoint;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, realGrapplePoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        if (grabbedTransform != null)
        {
            return grabbedTransform.TransformPoint(grabbedOffset);
        }
        return grapplePoint;
    }
}