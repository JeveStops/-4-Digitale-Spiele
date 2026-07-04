using UnityEngine;

public class SpiderBodySurfaceAligner : MonoBehaviour
{
    [Header("Ray Points")]
    [SerializeField] private Transform frontLeftFoot;
    [SerializeField] private Transform frontRightFoot;
    [SerializeField] private Transform backLeftFoot;
    [SerializeField] private Transform backRightFoot;

    [Header("Body")]
    [SerializeField] private float bodyHeight = 0.8f;
    [SerializeField] private float heightSmooth = 6f;
    [SerializeField] private float rotationSmooth = 6f;

    private void LateUpdate()
    {
        Vector3 averageFootPosition =
            (frontLeftFoot.position +
             frontRightFoot.position +
             backLeftFoot.position +
             backRightFoot.position) / 4f;

        Vector3 leftAverage =
            (frontLeftFoot.position + backLeftFoot.position) / 2f;

        Vector3 rightAverage =
            (frontRightFoot.position + backRightFoot.position) / 2f;

        Vector3 frontAverage =
            (frontLeftFoot.position + frontRightFoot.position) / 2f;

        Vector3 backAverage =
            (backLeftFoot.position + backRightFoot.position) / 2f;

        Vector3 rightDirection = (rightAverage - leftAverage).normalized;
        Vector3 forwardDirection = (frontAverage - backAverage).normalized;

        Vector3 surfaceNormal = Vector3.Cross(forwardDirection, rightDirection).normalized;

        if (surfaceNormal.y < 0f)
            surfaceNormal = -surfaceNormal;

        Vector3 targetPosition = averageFootPosition + surfaceNormal * bodyHeight;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * heightSmooth
        );

        Quaternion targetRotation = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(transform.forward, surfaceNormal),
            surfaceNormal
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmooth
        );
    }
}