using UnityEngine;
using UnityEngine.AI;

public class HunterNPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private BoidNPC[] allBoids;
    private HunterNPC[] allHunters;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Alle Boids und alle Jäger in der Szene finden
        allBoids = FindObjectsByType<BoidNPC>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        allHunters = FindObjectsByType<HunterNPC>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    void Update()
    {
        // Sicherheitshalber: Wenn es keine Herde gibt, mach nichts.
        if (allBoids.Length == 0) return;

        // 1. Mein persönlich nächstgelegenes Ziel finden
        float myMinDistance = GetDistanceToClosestBoid(transform.position, out Transform myTarget);

        if (myTarget == null) return;

        // 2. Ist ein anderer Jäger in der Szene noch näher an GENAU DIESEM Ziel dran als ich?
        bool amIClosestToMyTarget = true;
        foreach (var otherHunter in allHunters)
        {
            if (otherHunter == this) continue; // Mich selbst überspringen

            // Wir prüfen, wie weit der andere Jäger von MEINEM Ziel entfernt ist
            float distFromOtherToMyTarget = Vector3.Distance(otherHunter.transform.position, myTarget.position);

            // Wenn der andere Jäger näher an meinem Ziel ist, überlasse ich es ihm!
            if (distFromOtherToMyTarget < myMinDistance)
            {
                amIClosestToMyTarget = false;
                break; // Schleife abbrechen, wir müssen nicht weiter suchen
            }
        }

        // 3. Handeln!
        if (amIClosestToMyTarget)
        {
            // Niemand ist näher an diesem speziellen Boid als ich, also jage ich es!
            agent.isStopped = false; // NavMeshAgent wieder aktivieren falls er gestoppt war
            agent.SetDestination(myTarget.position);
        }
        else
        {
            // Jemand anderes kümmert sich bereits um diese Herde/diesen Boid. Ich bleibe stehen.
            agent.isStopped = true;
        }
    }

    // Hilfsfunktion: Berechnet den Abstand zum nächstgelegenen Boid von einer bestimmten Position aus
    float GetDistanceToClosestBoid(Vector3 originPos, out Transform closestBoid)
    {
        closestBoid = null;
        float minDistance = Mathf.Infinity;

        foreach (var boid in allBoids)
        {
            float dist = Vector3.Distance(originPos, boid.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestBoid = boid.transform;
            }
        }
        return minDistance;
    }
}