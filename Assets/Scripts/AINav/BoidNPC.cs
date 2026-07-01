using UnityEngine;
using UnityEngine.AI;

public class BoidNPC : MonoBehaviour
{
    // Die FSM-Zustände für die Herde
    public enum State { Flock, Flee }
    public State currentState = State.Flock;

    // HINWEIS: Das Feld "public Transform hunter" haben wir entfernt, 
    // da sich die Boids jetzt automatisch alle Jäger in der Szene suchen!

    public float fleeDistance = 8f;      // Ab wann wird geflohen?
    public float neighborRadius = 6f;    // Wie weit wird nach Nachbarn gesucht?
    public float separationDistance = 2f; // Mindestabstand zueinander

    private NavMeshAgent agent;
    private BoidNPC[] allBoids;
    private HunterNPC[] allHunters; // NEU: Eine Liste aller Jäger

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        allBoids = FindObjectsByType<BoidNPC>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        allHunters = FindObjectsByType<HunterNPC>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    void Update()
    {
        // 1. Wahrnehmung: Welcher Jäger von allen ist uns am nächsten?
        float closestHunterDist = Mathf.Infinity;
        Transform closestHunter = null;

        foreach (var h in allHunters)
        {
            float dist = Vector3.Distance(transform.position, h.transform.position);
            if (dist < closestHunterDist)
            {
                closestHunterDist = dist;
                closestHunter = h.transform;
            }
        }

        // 2. FSM Zustandswechsel (Wir reagieren nur auf den Jäger, der uns am nächsten ist)
        if (closestHunterDist < fleeDistance)
        {
            currentState = State.Flee; // Jäger ist nah -> Flucht!
        }
        else
        {
            currentState = State.Flock; // Jäger ist weit weg -> Herde bilden
        }

        // 3. Verhalten ausführen
        Vector3 targetPos = transform.position;

        if (currentState == State.Flock)
        {
            targetPos = CalculateFlockBehavior();
        }
        else if (currentState == State.Flee && closestHunter != null)
        {
            // Weg vom nächsten Jäger rennen...
            Vector3 fleeDirection = (transform.position - closestHunter.position).normalized;
            // ...aber trotzdem versuchen, bei der Gruppe zu bleiben
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

        foreach (var boid in allBoids)
        {
            if (boid == this) continue; // Sich selbst ignorieren

            float dist = Vector3.Distance(transform.position, boid.transform.position);

            if (dist < neighborRadius)
            {
                cohesion += boid.transform.position;
                neighbors++;

                // Wenn ein Kollege zu nah ist, baue Abstand auf
                if (dist < separationDistance)
                {
                    separation += (transform.position - boid.transform.position).normalized / dist;
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
        foreach (var boid in allBoids)
        {
            globalCenter += boid.transform.position;
        }
        globalCenter /= allBoids.Length;

        return globalCenter;
    }

    // Hilfsfunktion: Berechnet nur das Zentrum der Gruppe (für die gemeinsame Flucht)
    Vector3 CalculateCohesion()
    {
        Vector3 cohesion = Vector3.zero;
        int neighbors = 0;
        foreach (var boid in allBoids)
        {
            if (boid == this) continue;
            if (Vector3.Distance(transform.position, boid.transform.position) < neighborRadius)
            {
                cohesion += boid.transform.position;
                neighbors++;
            }
        }
        return neighbors > 0 ? cohesion / neighbors : transform.position;
    }
}