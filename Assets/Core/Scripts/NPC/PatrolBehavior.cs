using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCController))]
public class PatrolBehavior : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentPoint = 0;
    private NavMeshAgent agent;
    private NPCController animController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<NPCController>();
        
        agent.autoBraking = false; 

        GotoNextPoint();
    }

    void Update()
    {
        // Patrol Logic
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GotoNextPoint();
        }

        // Animation Logic
        if (agent.velocity.sqrMagnitude > 0.1f) 
        {
            animController.StartWalking(); 
        } 
        else 
        {
            animController.StopWalking(); 
        }
    }

    void GotoNextPoint()
    {
        if (waypoints.Length == 0) return;

        agent.destination = waypoints[currentPoint].position;
        currentPoint = (currentPoint + 1) % waypoints.Length;
    }
}