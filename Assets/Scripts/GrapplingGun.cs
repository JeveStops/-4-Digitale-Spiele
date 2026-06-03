using UnityEngine;
using TMPro; // NEU: Wir brauchen diesen Namespace für TextMeshPro!

public class GrapplingGun : MonoBehaviour
{

    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, camera, player;
    private float maxDistance = 100f;
    private SpringJoint joint;

    private Transform grabbedTransform;
    private Vector3 grabbedOffset;

    [Header("Shooting Setup")]
    public GameObject projectilePrefab;
    public float shootForce = 40f;
    public int maxMagSize = 5;
    private int currentMagSize;

    [Header("Laser Setup")]
    public LineRenderer laserLr;
    public float laserRange = 200f;
    public float laserForce = 500f;
    public float laserForceStep = 10f; // NEU: Um wie viel die Kraft pro Tastendruck steigt/fällt

    void Awake()
    {

        lr = GetComponent<LineRenderer>();

        if (laserLr != null)
        {
            laserLr.positionCount = 0;
        }

        currentMagSize = maxMagSize;
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

        // Rechtsklick: Projektil abfeuern
        if (Input.GetMouseButtonDown(1))
        {
            currentMagSize -= 1;
            if (currentMagSize > 0)
            {
                Shoot();
            }
        }

        //R-Taste: Lädt das Magazin nach
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload(currentMagSize);
        }

        // F-Taste HALTEN: Laser kontinuierlich updaten und abfeuern
        if (Input.GetKey(KeyCode.F))
        {
            UpdateLaser();
        }

        // F-Taste LOSLASSEN: Laser wieder verstecken
        if (Input.GetKeyUp(KeyCode.F))
        {
            DisableLaser();
        }

        // NEU: Laser-Stärke mit + und - regeln
        // Wir prüfen sowohl das Numpad (KeypadPlus) als auch das normale Plus (Plus)
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus))
        {
            laserForce += laserForceStep;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus))
        {
            laserForce -= laserForceStep;
            // Wir verhindern, dass die Kraft unter 0 fällt (macht bei einem Push-Laser wenig Sinn)
            if (laserForce < 0f)
            {
                laserForce = 0f;
            }
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

    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable))
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

    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
        grabbedTransform = null;
    }

    void Shoot()
    {
        if (projectilePrefab == null) return;

        GameObject bullet = Instantiate(projectilePrefab, gunTip.position, camera.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.AddForce(camera.forward * shootForce, ForceMode.Impulse);
        }

        Destroy(bullet, 5f);
    }

    void Reload(int magSize)
    {
        currentMagSize = maxMagSize;
    }

    void UpdateLaser()
    {
        if (laserLr == null) return;

        laserLr.positionCount = 2;
        laserLr.SetPosition(0, gunTip.position);

        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, laserRange))
        {
            laserLr.SetPosition(1, hit.point);

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(camera.forward * laserForce, ForceMode.Force);
            }
        }
        else
        {
            laserLr.SetPosition(1, camera.position + camera.forward * laserRange);
        }
    }

    void DisableLaser()
    {
        if (laserLr != null)
        {
            laserLr.positionCount = 0;
        }
    }

    private Vector3 currentGrapplePosition;

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