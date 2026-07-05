using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float forwardAngleLimit = 20f;

    private Vector3 targetDirection;
    private bool isMoving;

    public bool IsMoving => isMoving;

    private void Update()
    {

        if (!isMoving)
            return;

        Vector3 flatDirection = new Vector3(targetDirection.x, 0f, targetDirection.z).normalized;

        if (flatDirection == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(flatDirection, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );

        float angle = Vector3.Angle(transform.forward, flatDirection);

        if (angle <= forwardAngleLimit)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }

    public void SetMoveDirection(Vector3 direction)
    {
        targetDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        isMoving = targetDirection != Vector3.zero;
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        SetMoveDirection(direction);
    }

    public void Stop()
    {
        isMoving = false;
        targetDirection = Vector3.zero;
    }
}