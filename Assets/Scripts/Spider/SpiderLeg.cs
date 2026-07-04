using System.Collections;
using UnityEngine;

public class SpiderLeg : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spiderRoot;
    [SerializeField] private Transform footTarget;

    [Header("Foot Position")]
    [SerializeField] private Vector3 localOffset;

    [Header("Step Settings")]
    [SerializeField] private float stepDistance = 0.5f;
    [SerializeField] private float stepHeight = 0.25f;
    [SerializeField] private float stepDuration = 0.15f;

    [Header("Ground")]
    [SerializeField] private float raycastHeight = 1.5f;
    [SerializeField] private LayerMask groundLayer;

    public bool IsStepping { get; private set; }

    private void Start()
    {
        footTarget.position = GetGroundedTargetPosition();
    }

    public void TryStep()
    {
        if (IsStepping)
            return;

        Vector3 targetPosition = GetGroundedTargetPosition();

        if (Vector3.Distance(footTarget.position, targetPosition) > stepDistance)
        {
            StartCoroutine(StepTo(targetPosition));
        }
    }

    private Vector3 GetGroundedTargetPosition()
    {
        Vector3 worldPosition = spiderRoot.TransformPoint(localOffset);
        Vector3 rayOrigin = worldPosition + Vector3.up * raycastHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 3f, groundLayer))
        {
            return hit.point;
        }

        return worldPosition;
    }

    private IEnumerator StepTo(Vector3 targetPosition)
    {
        IsStepping = true;

        Vector3 startPosition = footTarget.position;
        float time = 0f;

        while (time < stepDuration)
        {
            float t = time / stepDuration;

            Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);
            position.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

            footTarget.position = position;

            time += Time.deltaTime;
            yield return null;
        }

        footTarget.position = targetPosition;
        IsStepping = false;
    }
}