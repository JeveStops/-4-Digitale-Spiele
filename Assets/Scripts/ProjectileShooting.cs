using UnityEngine;
using System.Collections; // WICHTIG für Coroutinen (IEnumerator)
using TMPro;

public class ProjectileShooting : MonoBehaviour
{
    [Header("References")]
    public Transform gunTip;
    public Transform playerCamera;
    public TMP_Text magazineText;

    [Header("Shooting Setup")]
    public GameObject projectilePrefab;
    public GameObject alternateProjectilePrefab;
    private GameObject currentProjectile;
    public float shootForce = 55f;

    [Header("Magazine & Reload")]
    public int maxMagSize = 5;
    private int currentMagSize;

    public float reloadTime = 0.5f;
    private bool isReloading = false;

    void Start()
    {
        currentProjectile = projectilePrefab;
        currentMagSize = maxMagSize;
        UpdateMagText();
    }

    void Update()
    {
        if (isReloading) return; // Verhindert weiteren Reload und Schießen während eines aktiven Reloads

        // Schießen
        if (Input.GetMouseButtonDown(1))
        {
            if (currentMagSize > 0)
            {
                currentMagSize--;
                Shoot(currentProjectile);
                UpdateMagText();
            }
            else
            {
                // Automatisches Nachladen, wenn man leer klickt
                StartCoroutine(ReloadRoutine());
            }
        }

        // Manuelles Nachladen
        if (Input.GetKeyDown(KeyCode.R) && currentMagSize < maxMagSize)
        {
            StartCoroutine(ReloadRoutine());
        }

        // Projektil wechseln
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentProjectile = projectilePrefab;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentProjectile = alternateProjectilePrefab;
    }

    void Shoot(GameObject projectile)
    {
        if (projectile == null) return;

        GameObject bullet = Instantiate(projectile, gunTip.position, playerCamera.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(playerCamera.forward * shootForce, ForceMode.Impulse);
        }

        Destroy(bullet, 5f);
    }

    // Coroutine für das zeitgesteuerte Nachladen
    IEnumerator ReloadRoutine()
    {
        isReloading = true; // Blockiert das Schießen

        // Pausiert die Ausführung genau hier für 'reloadTime' Sekunden
        yield return new WaitForSeconds(reloadTime);

        // Nach der Wartezeit: Magazin voll machen und UI updaten
        currentMagSize = maxMagSize;
        UpdateMagText();

        isReloading = false; // Schießen wieder erlauben
    }

    void UpdateMagText()
    {
        if (magazineText != null)
        {
            magazineText.text = currentMagSize + "/" + maxMagSize;
        }
    }
}