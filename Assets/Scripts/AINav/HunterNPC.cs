using UnityEngine;
using UnityEngine.AI;

public class HunterNPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private BoidNPC[] allBoids;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Findet alle Herden-NPCs in der Szene (Syntax für Unity 6)
        allBoids = FindObjectsByType<BoidNPC>();
    }

    void Update()
    {
        Transform closestBoid = null;
        float minDistance = Mathf.Infinity;

        // Finde den nächstliegenden NPC der Herde
        foreach (var boid in allBoids)
        {
            float dist = Vector3.Distance(transform.position, boid.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestBoid = boid.transform;
            }
        }

        // Verfolge das nächste Ziel
        if (closestBoid != null)
        {
            agent.SetDestination(closestBoid.position);
        }
    }
}