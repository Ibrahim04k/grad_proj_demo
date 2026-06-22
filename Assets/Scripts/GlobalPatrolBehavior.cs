using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCController))]
public class GlobalPatrolBehavior : MonoBehaviour
{
    private NavMeshAgent agent;
    private NPCController animController;
    private NavMeshTriangulation navMeshData; // Stores your baked NavMesh

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<NPCController>();
        agent.autoBraking = false; 

        // Grab all the walkable area data you baked in Unity
        navMeshData = NavMesh.CalculateTriangulation();

        GoToRandomNavMeshPoint();
    }

    void Update()
    {
        // Check if destination is reached
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToRandomNavMeshPoint();
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

    void GoToRandomNavMeshPoint()
    {
        // Safety check to ensure NavMesh data exists
        if (navMeshData.vertices.Length == 0) return;

        // Pick a random point directly from your baked walkable area
        int randomIndex = Random.Range(0, navMeshData.vertices.Length);
        Vector3 randomPoint = navMeshData.vertices[randomIndex];

        agent.destination = randomPoint;
    }
}