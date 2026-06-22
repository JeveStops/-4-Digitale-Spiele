using UnityEngine;

public class LaserGun : MonoBehaviour
{
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

    private void Awake()
    {
        if (laserLr != null)
        {
            laserLr.positionCount = 0;
        }

        if (laserBar != null)
        {
            laserBar.SetMaxLaserNRG(currentLaserTime);
        }

        currentLaserTime = maxLaserTime;
    }


    // Update is called once per frame
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
        if (Input.GetKey(KeyCode.F) && currentLaserTime > 0)
        {
            FireLaser();
        }
    }

    void FireLaser()
    {
        if (laserLr == null) return;

        laserLr.positionCount = 2;
        laserLr.SetPosition(0, gunTip.position);

        RaycastHit hit;
        // Und natürlich auch hier beim Laser: 'playerCamera'
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, laserRange))
        {
            laserLr.SetPosition(1, hit.point);

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(playerCamera.forward * laserForce, ForceMode.Force);
            }

            if (laserHitEffectPrefab != null)
            {
                if (activeLaserHitEffect == null)
                {
                    activeLaserHitEffect = Instantiate(laserHitEffectPrefab);
                }

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

        currentLaserTime -= laserDrainRate * Time.deltaTime;

        if (currentLaserTime < 0)
        {
            currentLaserTime = 0;
        }

        if (laserBar != null) laserBar.SetLaserNRG(currentLaserTime);
    }
}
