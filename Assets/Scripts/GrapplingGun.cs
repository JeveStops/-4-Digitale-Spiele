using UnityEngine;
using TMPro;

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
    public GameObject alternateProjectilePrefab;
    private GameObject currentProjectile;
    public float shootForce = 40f;
    public int maxMagSize = 5;
    private int currentMagSize;
    public TMP_Text magazineText;

    [Header("Laser Setup")]
    public LineRenderer laserLr;
    public float laserRange = 200f;
    public float laserForce = 500f;
    public float laserForceStep = 10f;
    public float maxLaserTime = 3000f;
    public float currentLaserTime;
    public LaserBar laserBar;

    // NEU: Hit Effekt Variablen
    public GameObject laserHitEffectPrefab;
    private GameObject activeLaserHitEffect; // Das ist unser recycelter Effekt

    public float laserDrainRate = 1000f;
    public float laserRechargeRate = 500f;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();

        if (laserLr != null)
        {
            laserLr.positionCount = 0;
        }

        currentMagSize = maxMagSize;
        currentLaserTime = maxLaserTime;

        if (laserBar != null)
        {
            laserBar.SetMaxLaserNRG(currentLaserTime);
        }

        currentProjectile = projectilePrefab;

        UpdateMagText();
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
            if (currentMagSize > 0)
            {
                currentMagSize -= 1;
                Shoot(currentProjectile);
            }
            UpdateMagText();
        }

        //1-Taste für Projektil 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentProjectile = projectilePrefab;
        }

        //2-Taste für Projektil 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentProjectile = alternateProjectilePrefab;
        }

        //R-Taste: Lädt das Magazin nach
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload(currentMagSize);
            UpdateMagText();
        }

        // F-Taste LOSLASSEN: Laser wieder verstecken
        if (Input.GetKeyUp(KeyCode.F) || currentLaserTime <= 0)
        {
            DisableLaser();
        }

        // Laser-Stärke mit + und - regeln
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus))
        {
            laserForce += laserForceStep;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus))
        {
            laserForce -= laserForceStep;
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

        // Passives Aufladen des Lasers
        if (currentLaserTime < maxLaserTime && !Input.GetKey(KeyCode.F))
        {
            currentLaserTime += laserRechargeRate * Time.deltaTime;

            if (currentLaserTime > maxLaserTime)
            {
                currentLaserTime = maxLaserTime;
            }

            if (laserBar != null) laserBar.SetLaserNRG(currentLaserTime);
        }
    }

    void LateUpdate()
    {
        DrawRope();

        if (Input.GetKey(KeyCode.F) && currentLaserTime > 0)
        {
            UpdateLaser();
        }
    }

    void UpdateMagText()
    {
        if (magazineText != null)
        {
            magazineText.text = currentMagSize + "/" + maxMagSize;
        }
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

    void Shoot(GameObject projectile)
    {
        if (projectilePrefab == null) return;

        GameObject bullet = Instantiate(projectile, gunTip.position, camera.rotation);
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

            // NEU: Hit Effekt positionieren und aktivieren
            if (laserHitEffectPrefab != null)
            {
                // Falls wir noch keinen Effekt erschaffen haben, tun wir das jetzt
                if (activeLaserHitEffect == null)
                {
                    activeLaserHitEffect = Instantiate(laserHitEffectPrefab);
                }

                activeLaserHitEffect.SetActive(true); // Einschalten
                activeLaserHitEffect.transform.position = hit.point; // Genau an die Trefferstelle setzen

                // Rotiert den Effekt so, dass Funken von der Wand WEC fliegen (hit.normal)
                activeLaserHitEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        }
        else
        {
            laserLr.SetPosition(1, camera.position + camera.forward * laserRange);

            // NEU: Wir schießen ins Nichts -> Effekt ausschalten
            if (activeLaserHitEffect != null)
            {
                activeLaserHitEffect.SetActive(false);
            }
        }

        currentLaserTime -= laserDrainRate * Time.deltaTime;

        if (currentLaserTime < 0)
        {
            currentLaserTime = 0;
        }

        if (laserBar != null) laserBar.SetLaserNRG(currentLaserTime);
    }

    void DisableLaser()
    {
        if (laserLr != null)
        {
            laserLr.positionCount = 0;
        }

        // NEU: Laser ist aus -> Effekt ausschalten
        if (activeLaserHitEffect != null)
        {
            activeLaserHitEffect.SetActive(false);
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