using UnityEngine;

public class DanceCameraOrbit : MonoBehaviour
{
    public Transform target;

    public float distance = 4f;
    public float height = 1.5f;
    public float sensitivity = 120f;

    private float yaw;
    private float pitch = 15f;

    public void StartOrbitFromCurrentView(Transform currentCamera)
    {
        yaw = currentCamera.eulerAngles.y;
        pitch = currentCamera.eulerAngles.x;

        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -20f, 60f);

        UpdateCameraPosition();
    }

    void Update()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -20f, 60f);

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + Vector3.up * height + offset;

        transform.LookAt(target.position + Vector3.up * height);
    }
}