using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 5f;

    private Vector3 moveDirection;
    private bool isMoving;

    public bool IsMoving => isMoving;

    private void Update()
    {
        if (!isMoving)
            return;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    public void SetMoveDirection(Vector3 direction)
    {
        moveDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        isMoving = moveDirection != Vector3.zero;
    }

    public void Stop()
    {
        isMoving = false;
    }
}