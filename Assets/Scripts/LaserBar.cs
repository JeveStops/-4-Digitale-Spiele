using UnityEngine;
using UnityEngine.UI;

public class LaserBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxLaserNRG(float laserTime)
    {
        slider.maxValue = laserTime;
        slider.value = laserTime;
    }

    public void SetLaserNRG(float laserTime)
    {
        slider.value = laserTime;
    }
}
