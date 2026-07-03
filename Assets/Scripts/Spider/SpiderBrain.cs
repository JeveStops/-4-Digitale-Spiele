using System.Collections;
using UnityEngine;

public class SpiderBrain : MonoBehaviour
{
    [SerializeField] private SpiderMovement movement;

    [SerializeField] private float minWalkTime = 2f;
    [SerializeField] private float maxWalkTime = 5f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    private void Start()
    {
        StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            movement.SetMoveDirection(GetRandomDirection());

            yield return new WaitForSeconds(UnityEngine.Random.Range(minWalkTime, maxWalkTime));

            movement.Stop();

            yield return new WaitForSeconds(UnityEngine.Random.Range(minIdleTime, maxIdleTime));
        }
    }

    private Vector3 GetRandomDirection()
    {
        float angle = UnityEngine.Random.Range(0f, 360f);
        return Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
    }
}