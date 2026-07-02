using TMPro;
using UnityEngine;

public class ProjectileShooting : MonoBehaviour
{
    public Transform gunTip, playerCamera;

    [Header("Shooting Setup")]
    public GameObject projectilePrefab;
    public GameObject alternateProjectilePrefab;
    private GameObject currentProjectile;
    public float shootForce = 40f;
    public int maxMagSize = 5;
    private int currentMagSize;
    public TMP_Text magazineText;

    [Header("Effects")]
    [SerializeField] private GunMuzzleFlash muzzleFlash;

    private void Awake()
    {
        currentMagSize = maxMagSize;

        currentProjectile = projectilePrefab;

        UpdateMagText();
    }

    void Update()
    {
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
    }

    void Shoot(GameObject projectile)
    {
        if (projectile == null) return;

        GameObject bullet = Instantiate(projectile, gunTip.position, playerCamera.rotation);

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.AddForce(playerCamera.forward * shootForce, ForceMode.Impulse);
        }

        muzzleFlash?.Fire();

        Destroy(bullet, 5f);
    }

    void Reload(int magSize)
    {
        currentMagSize = maxMagSize;
    }

    void UpdateMagText()
    {
        if (magazineText != null)
        {
            magazineText.text = currentMagSize + "/" + maxMagSize;
        }
    }
}
