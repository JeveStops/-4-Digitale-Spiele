using UnityEngine;
using UnityEngine.AI;

public class NPC_FSM : MonoBehaviour
{
    // 1. Drei Zustände der FSM
    public enum State { Wander, Seek, Hunt }

    // Aktueller Zustand (Standard ist Wander)
    public State currentState = State.Wander;

    // 2. Unsere Ziele für die anderen Zustände Seek und Hunt
    public Transform seekTarget;
    public Transform huntTarget;

    // 3. Entfernungen, bei denen der Zustand gewechselt wird
    public float huntDistance = 5f;  // Wenn Spieler näher als 5m ist, Hunt
    public float seekDistance = 15f; // Wenn Zielobjekt näher als 15m ist, Seek
    public float wanderRadius = 10f; // Wie weit darf er beim Wandern laufen?

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GetNewWanderPoint(); // NPC soll direkt loslaufen
    }

    void Update()
    {
        CheckTransitions(); // Prüfe jeden Frame, ob wir den Zustand wechseln müssen

        // 4. Verhalten je nach Zustand
        switch (currentState)
        {
            case State.Wander:
                // Wenn der NPC an seinem zufälligen Ziel angekommen ist, such ein neues
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    GetNewWanderPoint();
                }
                break;

            case State.Seek:
                agent.SetDestination(seekTarget.position);
                break;

            case State.Hunt:
                agent.SetDestination(huntTarget.position);
                break;
        }
    }

    // 5. Die Logik für den Zustandswechsel (Entscheidungsfindung)
    void CheckTransitions()
    {
        float distToHunt = Vector3.Distance(transform.position, huntTarget.position);
        float distToSeek = Vector3.Distance(transform.position, seekTarget.position);

        // Priorität 1: Wenn der Spieler nah ist, Jagen
        if (distToHunt <= huntDistance)
        {
            currentState = State.Hunt;
        }
        // Priorität 2: Wenn ein Zielobjekt in der Nähe ist, Suchen
        else if (distToSeek <= seekDistance)
        {
            currentState = State.Seek;
        }
        // Ansonsten: Einfach herumwandern.
        else
        {
            currentState = State.Wander;
        }
    }

    // 6. Hilfsfunktion für zufälliges Umherwandern auf dem NavMesh
    void GetNewWanderPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        // Checkt, ob der zufällige Punkt wirklich auf dem NavMesh liegt
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }
}