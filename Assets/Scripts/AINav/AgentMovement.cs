using UnityEngine;
using UnityEngine.AI;

public class AgentMovement : MonoBehaviour
{
    public Transform target; // Ziel Objekt, was der Agent

    private NavMeshAgent agent;

    void Start()
    {
        // Zugriff auf Agent-Komponente holen
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Verfolgung wird pro Frame erneuert, sodass auch bei Verschiebung des Objekt der Agent die aktuelleste Position verfolgt
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}