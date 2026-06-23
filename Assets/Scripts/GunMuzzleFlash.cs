using System.Collections;
using UnityEngine;

public class GunMuzzleFlash : MonoBehaviour
{
    [SerializeField] private Light muzzleLight1;
    [SerializeField] private Light muzzleLight2;
    [SerializeField] private float flashDuration = 0.05f;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Sicherstellen, dass die Lichter beim Start aus sind
        muzzleLight1.enabled = false;
        muzzleLight2.enabled = false;
    }

    public void Fire()
    {
        // Falls schnell hintereinander geschossen wird:
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashLight());
    }

    private IEnumerator FlashLight()
    {
        muzzleLight1.enabled = true;
        muzzleLight2.enabled = true;

        yield return new WaitForSeconds(flashDuration);

        muzzleLight1.enabled = false;
        muzzleLight2.enabled = false;
        flashCoroutine = null;
    }
}
