using System.Collections;
using UnityEngine;

public class SpiderBrain : MonoBehaviour
{
    public enum SpiderState
    {
        Patrol,
        Follow,
        Freeze,
    }

    [Header("References")]
    [SerializeField] private SpiderMovement movement;
    [SerializeField] private SpiderLegController legController;
    [SerializeField] private SpiderBodySurfaceAligner bodyAligner;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;
    [SerializeField] private EntityMovement playerMovement;

    [Header("Patrol")]
    [SerializeField] private float minWalkTime = 2f;
    [SerializeField] private float maxWalkTime = 5f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Freeze")]
    [SerializeField] private float freezeDistance = 2f;

    [Header("State")]
    [SerializeField] private SpiderState currentState = SpiderState.Freeze;
    private Coroutine patrolRoutine;

    private Vector3 frozenPosition;
    private Quaternion frozenRotation;

    private void Start()
    {
        SetState(SpiderState.Patrol);

    }

    private void Update()
    {
        bool seesPlayer = CanSeePlayer();
        float distanceToPlayer = player == null ? Mathf.Infinity : Vector3.Distance(transform.position, player.position);

        
        if (seesPlayer && distanceToPlayer <= freezeDistance)
        {
            SetState(SpiderState.Freeze);
        }
        else if (seesPlayer)
        {
            SetState(SpiderState.Follow);
        }
        else
        {
            SetState(SpiderState.Patrol);
        }
        

        switch (currentState)
        {
            case SpiderState.Follow:
                movement.MoveTowards(player.position);
                break;

            case SpiderState.Freeze:
                movement.Stop();
                transform.position = frozenPosition;
                transform.rotation = frozenRotation;
                break;
        }

        HandleSpiderDance();
    }

    private void SetState(SpiderState newState)
    {
        if (currentState == newState)
            return;

        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(SpiderState state)
    {
        switch (state)
        {
            case SpiderState.Patrol:
                Debug.Log("Patrol!");
                EnableProceduralAnimation(true);
                patrolRoutine = StartCoroutine(PatrolRoutine());
                break;

            case SpiderState.Follow:
            Debug.Log("Follow!");
                EnableProceduralAnimation(true);
                StopPatrolRoutine();
                break;

            case SpiderState.Freeze:
                Debug.Log("Freeze!");
                StopPatrolRoutine();
                FreezePose();
                EnableProceduralAnimation(false);

                if (animator != null) 
                {
                animator.SetBool("Freeze", true);
                animator.SetBool("Waving", true);
                }
                break;
        }
    }

    private void ExitState(SpiderState state)
    {
        switch (state)
        {
            case SpiderState.Patrol:
                StopPatrolRoutine();
                break;

            case SpiderState.Freeze:
                if (animator != null)
                {
                    animator.SetBool("Freeze", false);
                    animator.SetBool("Waving", false);
                    animator.SetBool("Dancing", false);
                }
                break;
        }
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

    private bool CanSeePlayer()
    {
        if (player == null) {
            return false;
        }

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > viewDistance) {
            return false;
        }

        Vector3 flatDirection = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;
        float angle = Vector3.Angle(transform.forward, flatDirection);

        if (angle > viewAngle * 0.5f) {
            return false;
        }

        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 rayTarget = player.position + Vector3.up * 0.5f;
        Vector3 rayDirection = rayTarget - rayStart;

        if (Physics.Raycast(rayStart, rayDirection.normalized, out RaycastHit hit, viewDistance, obstacleLayer))
        {
            return false;
        }
        return true;
    }

    private void FreezePose()
    {
        frozenPosition = transform.position;
        frozenRotation = transform.rotation;
        movement.Stop();
    }

    private void EnableProceduralAnimation(bool enabled)
    {
        if (legController != null)
            legController.enabled = enabled;
        
        if (bodyAligner != null)
            bodyAligner.enabled = enabled;
           
    }

    private void StopPatrolRoutine()
    {
        if (patrolRoutine != null)
        {
            StopCoroutine(patrolRoutine);
            patrolRoutine = null;
        }
    }

    public void StopWaving()
    {
        animator.SetBool("Waving", false);
    }

    private void HandleSpiderDance()
    {
        if (animator == null || playerMovement == null)
            return;

        bool isFrozen = currentState == SpiderState.Freeze;
        bool isWaving = animator.GetBool("Waving");
        bool playerIsDancing = playerMovement.IsDancing;

        bool spiderShouldDance = isFrozen && !isWaving && playerIsDancing;

        animator.SetBool("Dancing", spiderShouldDance);
    }

}