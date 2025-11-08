using UnityEngine;
using UnityEngine.AI;

public class TheEnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, escape }
    private State currentState = State.Patrol;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] Transform player;
    [SerializeField] float chaseDistance = 5f;
    [SerializeField] float escapeDistance = 3f;
    private NavMeshAgent agent;
    private int currentIndex = 0;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        Patrol();
    }
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                agent.speed = 2f;
                if (distanceToPlayer <= chaseDistance)
                    currentState = State.Chase;
                break;
            case State.Chase:
                Chase();
                agent.speed = 4f;
                if (distanceToPlayer > chaseDistance)
                    currentState = State.Patrol;
                break;
                // case State.Escape:
                //     Escape();
                //     agent.speed = 6f;
                //     break;
        }
    }
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.SetDestination(patrolPoints[currentIndex].position);
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
        }
    }
    void Chase()
    {
        agent.SetDestination(player.position);
    }
    // void Escape()
    // {
    //     // Escape logic
    //     // animation
    // }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}