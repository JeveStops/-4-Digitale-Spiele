using UnityEngine;
using UnityEngine.AI; // Wichtig für die Navigation!

public class AgentMovement : MonoBehaviour
{
    // Hier ziehen wir später im Inspector unser Ziel-Objekt rein
    public Transform target;

    private NavMeshAgent agent;

    void Start()
    {
        // Wir holen uns die Agenten-Komponente, die auf derselben Kapsel liegt
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Wir sagen dem Agenten jeden Frame: "Gehe zur Position des Ziels!"
        // So läuft er auch weiter, wenn du das Ziel im Play-Modus verschiebst.
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}