using UnityEngine;
using UnityEngine.AI;

public class HunterNPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private FlockNPC[] allFlockNPCs;
    private HunterNPC[] allHunters;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        allFlockNPCs = FindObjectsByType<FlockNPC>(FindObjectsInactive.Exclude);
        allHunters = FindObjectsByType<HunterNPC>(FindObjectsInactive.Exclude);
    }

    void Update()
    {
        // Wenn es keine Herde gibt, mach nichts.
        if (allFlockNPCs.Length == 0) return;

        // 1. Nächstgelegenes Ziel finden
        float minDistance = GetDistanceToClosestNPC(transform.position, out Transform target);

        if (target == null) return;

        // 2. Ist ein anderer Jäger in der Szene noch näher an GENAU DIESEM Ziel dran?
        bool amIClosestToMyTarget = true;
        foreach (var otherHunter in allHunters)
        {
            if (otherHunter == this) continue; // Sich selbst überspringen

            // Wir prüfen, wie weit der andere Jäger vom eigenen Ziel entfernt ist
            float distFromOtherToMyTarget = Vector3.Distance(otherHunter.transform.position, target.position);

            // Wenn der andere Jäger näher am Ziel ist, überlasse es dem anderen
            if (distFromOtherToMyTarget < minDistance)
            {
                amIClosestToMyTarget = false;
                break; // Schleife abbrechen, wir müssen nicht weiter suchen
            }
        }

        // 3. Verhalten ausführen
        if (amIClosestToMyTarget)
        {
            // Niemand ist näher an diesem speziellen Herden Mitglied als ich, also jage ich es!
            agent.isStopped = false; // NavMeshAgent wieder aktivieren falls er gestoppt war
            agent.SetDestination(target.position);
        }
        else
        {
            // Jemand anderes kümmert sich bereits um diese Herde/diesen Boid. Ich bleibe stehen.
            agent.isStopped = true;
        }
    }

    // Hilfsfunktion: Berechnet den Abstand zum nächstgelegenen Herden Mitglied von einer bestimmten Position aus
    float GetDistanceToClosestNPC(Vector3 originPos, out Transform closestFlockNPC)
    {
        closestFlockNPC = null;
        float minDistance = Mathf.Infinity;

        foreach (var npc in allFlockNPCs)
        {
            float dist = Vector3.Distance(originPos, npc.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestFlockNPC = npc.transform;
            }
        }
        return minDistance;
    }
}