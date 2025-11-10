using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class TheEnemyAI : MonoBehaviour
{
    private enum State { Spawn, Chase, Attack, Panic, Escape }
    [SerializeField] private State currentState = State.Spawn;

    [Header("Patrol Settings")]
    [SerializeField] Transform[] patrolPoints;

    [Header("Detection Settings")]
    [SerializeField] float chaseDistance = 10f;
    [SerializeField] float attackDistance = 2f;
    [SerializeField] float escapeDistance = 15f;

    [Header("Combat Settings")]
    [SerializeField] float attackRate = 1.5f;
    private float nextAttackTime = 0f;

    [Header("Panic Settings")]
    [SerializeField] float panicDuration = 5f;
    [SerializeField] GameObject blindParticlePrefab;
    private NavMeshAgent agent;
    private Transform targetPlayer;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        FindNearestPlayer();
        float distanceToPlayer = targetPlayer ? Vector3.Distance(transform.position, targetPlayer.position) : float.MaxValue;

        if (currentState != State.Panic && currentState != State.Escape)
        {
            if (distanceToPlayer <= attackDistance)
            {
                currentState = State.Attack;
            }
            else if (distanceToPlayer <= chaseDistance)
            {
                currentState = State.Chase;
            }
            else
            {
                currentState = State.Spawn;
            }
        }
        switch (currentState)
        {
            case State.Spawn:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;

            case State.Escape:
                Escape();
                break;

            case State.Panic:
                break;
        }
    }
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[0].position);
    }
    void Chase()
    {
        if (targetPlayer == null) return;
        agent.isStopped = false;
        agent.SetDestination(targetPlayer.position);
    }

    void Attack()
    {
        if (targetPlayer == null) return;
        agent.isStopped = true;
        transform.LookAt(targetPlayer);

        if (Time.time >= nextAttackTime)
        {
            Debug.Log("Enemy Attacks " + targetPlayer.name);
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }
    public void TriggerBlindness()
    {
        if (currentState == State.Panic || currentState == State.Escape) return;
        StartCoroutine(PanicRoutine());
    }
    IEnumerator PanicRoutine()
    {
        currentState = State.Panic;
        agent.isStopped = true;
        if (blindParticlePrefab != null)
        {
            Instantiate(blindParticlePrefab, transform.position, Quaternion.identity);
        }
        Debug.Log("The Enemy is Blinded and Panicking!");
        yield return new WaitForSeconds(panicDuration);
        currentState = State.Escape;
        Debug.Log("The Enemy is Escaping!");
    }
    void Escape()
    {
        if (targetPlayer == null)
        {
            currentState = State.Spawn;
            return;
        }
        Vector3 directionToEscape = transform.position - targetPlayer.position;
        Vector3 escapePoint = transform.position + directionToEscape.normalized * escapeDistance;
        agent.isStopped = false;
        agent.SetDestination(escapePoint);

        if (Vector3.Distance(transform.position, escapePoint) < 1.5f)
        {
            currentState = State.Spawn;
            Debug.Log("The Enemy has Escaped and is Resuming Patrol.");
        }
    }
    void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float shortestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;
        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < shortestDistance)
            {
                shortestDistance = distanceToPlayer;
                nearestPlayer = player.transform;
            }
        }
        if (nearestPlayer != null)
        {
            targetPlayer = nearestPlayer;
        }
        else
        {
            targetPlayer = null;
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}