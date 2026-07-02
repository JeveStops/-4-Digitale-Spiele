using UnityEngine;
using UnityEngine.AI;

public class FlockNPC : MonoBehaviour
{
    // Die FSM-Zustände für die Herde
    public enum State { Flock, Flee }
    public State currentState = State.Flock;

    public float fleeDistance = 8f;      // Ab wann wird geflohen?
    public float neighborRadius = 6f;    // Wie weit wird nach Nachbarn gesucht?
    public float separationDistance = 2f; // Mindestabstand zueinander

    private NavMeshAgent agent;
    private FlockNPC[] allFlockNPCs; //Speichert alle Herden Mitglieder
    private HunterNPC[] allHunters; //Speichert alle Jäger Objekte

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        allFlockNPCs = FindObjectsByType<FlockNPC>(FindObjectsInactive.Exclude);
        allHunters = FindObjectsByType<HunterNPC>(FindObjectsInactive.Exclude);
    }

    void Update()
    {
        // 1. Untersuchung: Prüfung, welcher Jäger der Herde am nächsten ist

        // Initialisierung
        float closestHunterDist = Mathf.Infinity;
        Transform closestHunter = null;

        foreach (var hunter in allHunters)
        {
            float dist = Vector3.Distance(transform.position, hunter.transform.position); // Distanz zwischen eigener und Jäger Position
            if (dist < closestHunterDist)
            {
                closestHunterDist = dist;
                closestHunter = hunter.transform;
            }
        }

        // 2. FSM Zustandswechsel: Reaktion nur auf den naheliegendsten Jäger
        if (closestHunterDist < fleeDistance)
        {
            currentState = State.Flee; // Ist der Jäger zu nah, Fliehen
        }
        else
        {
            currentState = State.Flock; // Ist der Jäger weit genug weg, Herde bilden
        }

        // 3. Verhalten ausführen
        Vector3 targetPos = transform.position;

        if (currentState == State.Flock)
        {
            targetPos = CalculateFlockBehavior();
        }
        else if (currentState == State.Flee && closestHunter != null)
        {
            // Weg vom nächsten Jäger fliehen, ...
            Vector3 fleeDirection = (transform.position - closestHunter.position).normalized;

            // ...aber trotzdem versuchen, bei der Herde zu bleiben
            Vector3 groupCenter = CalculateCohesion();

            targetPos = transform.position + (fleeDirection * 5f) + (groupCenter - transform.position).normalized * 2f;
        }

        // Damit der Punkt immer gültig auf dem NavMesh liegt
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 4f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // Berechnet das Zusammenfinden und den Abstand
    Vector3 CalculateFlockBehavior()
    {
        Vector3 cohesion = Vector3.zero; // Zusammenfinden
        Vector3 separation = Vector3.zero; // Abstand halten
        int neighbors = 0;

        foreach (var npc in allFlockNPCs)
        {
            if (npc == this) continue; // Sich selbst ignorieren

            float dist = Vector3.Distance(transform.position, npc.transform.position);

            if (dist < neighborRadius)
            {
                cohesion += npc.transform.position;
                neighbors++;

                // Wenn ein anderes Herden Mitglied zu nah ist, baue Abstand auf
                if (dist < separationDistance)
                {
                    separation += (transform.position - npc.transform.position).normalized / dist;
                }
            }
        }

        if (neighbors > 0)
        {
            cohesion /= neighbors; // Berechne die Mitte der Nachbarn
            return cohesion + separation; // Ziel ist die Mitte + nötiger Abstand
        }

        // Wenn niemand in der Nähe ist, rette dich zur globalen Mitte der gesamten Herde!
        Vector3 globalCenter = Vector3.zero;
        foreach (var npc in allFlockNPCs)
        {
            globalCenter += npc.transform.position;
        }
        globalCenter /= allFlockNPCs.Length;

        return globalCenter;
    }

    // Hilfsfunktion: Berechnet nur das Zentrum der Gruppe (für die gemeinsame Flucht)
    Vector3 CalculateCohesion()
    {
        Vector3 cohesion = Vector3.zero;
        int neighbors = 0;
        foreach (var npc in allFlockNPCs)
        {
            if (npc == this) continue;
            if (Vector3.Distance(transform.position, npc.transform.position) < neighborRadius)
            {
                cohesion += npc.transform.position;
                neighbors++;
            }
        }
        return neighbors > 0 ? cohesion / neighbors : transform.position;
    }
}