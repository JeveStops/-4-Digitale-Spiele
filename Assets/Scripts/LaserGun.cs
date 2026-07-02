using UnityEngine;

public class LaserGun : MonoBehaviour
{
    public Transform gunTip, playerCamera;

    [Header("Laser Setup")]
    public LineRenderer laserLr;
    public float laserRange = 200f;
    public float laserForce = 500f;
    public float laserForceStep = 10f;
    public float maxLaserTime = 3000f;
    public float currentLaserTime;
    public LaserBar laserBar;
    public float laserDrainRate = 1000f;
    public float laserRechargeRate = 500f;

    // Hit Effekt Variablen
    public GameObject laserHitEffectPrefab;
    private GameObject activeLaserHitEffect;

    // Initialisierung
    private void Awake()
    {
        if (laserLr != null)
        {
            laserLr.positionCount = 0;
        }

        
        currentLaserTime = maxLaserTime;

        if (laserBar != null)
        {
            laserBar.SetMaxLaserNRG(maxLaserTime);
        }
    }


    void Update()
    {
        // F-Taste LOSLASSEN: Laser wieder verstecken
        if (Input.GetKeyUp(KeyCode.F) || currentLaserTime <= 0)
        {
            DisableLaser();
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

    private void LateUpdate()
    {
        // F-Taste GEDR‹CKT HALTEN: Laser abfeuern
        if (Input.GetKey(KeyCode.F) && currentLaserTime > 0)
        {
            FireLaser();
        }
    }

    // Abgefeurter Laser  stoﬂt Objekte mit Rigidbodies weg
    void FireLaser()
    {
        if (laserLr == null) return;

        laserLr.positionCount = 2;
        laserLr.SetPosition(0, gunTip.position);

        RaycastHit hit;
        
        // Verhalten, wenn der Laser einen Rigibody trifft oder nichts

        // 1. Laser trifft Rigidbody
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, laserRange))
        {
            laserLr.SetPosition(1, hit.point); // Konfigurierung der Laser-Visualisierung

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(playerCamera.forward * laserForce, ForceMode.Force); // Stoﬂ-Kraft auf den getroffeen Rigidbody anwenden
            }

            if (laserHitEffectPrefab != null)
            {
                if (activeLaserHitEffect == null)
                {
                    activeLaserHitEffect = Instantiate(laserHitEffectPrefab); // Laser-Effekt erstellen falls vorhanden
                }

                // Laser-Effekt aktivieren an der Einschalgsstelle des Lasers
                activeLaserHitEffect.SetActive(true);
                activeLaserHitEffect.transform.position = hit.point;
                activeLaserHitEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        }
        else
        {
            laserLr.SetPosition(1, playerCamera.position + playerCamera.forward * laserRange);

            if (activeLaserHitEffect != null)
            {
                activeLaserHitEffect.SetActive(false);
            }
        }

        currentLaserTime -= laserDrainRate * Time.deltaTime; // Reduktion der Laser-Energie je l‰nger der Laser abgefeurt wird

        if (currentLaserTime < 0)
        {
            currentLaserTime = 0;
        }

        if (laserBar != null) laserBar.SetLaserNRG(currentLaserTime); // Verkn¸pfung mit der UI
    }

    void DisableLaser()
    {
        if (laserLr != null)
        {
            laserLr.positionCount = 0;
        }

        if (activeLaserHitEffect != null)
        {
            activeLaserHitEffect.SetActive(false);
        }
    }
}