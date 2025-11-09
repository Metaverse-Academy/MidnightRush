using UnityEngine;
using UnityEngine.AI;
using System.Collections; // **هذا هو التصحيح الرئيسي**
using System.Linq; 

public class TheEnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack, Panic, Escape }
    private State currentState = State.Patrol;
    
    [Header("Patrol Settings")]
    [SerializeField] Transform[] patrolPoints;
    
    [Header("Detection Settings")]
    [SerializeField] float chaseDistance = 5f;
    [SerializeField] float attackDistance = 2f;
    [SerializeField] float escapeSpeed = 6f; 
    
    [Header("Panic Settings")]
    [SerializeField] float panicDuration = 2f; 
    [SerializeField] GameObject blindParticlePrefab; 
    
    // متغيرات داخلية
    private NavMeshAgent agent;
    private int currentIndex = 0;
    private Transform targetPlayer; 
    private Collider[] allPlayers; 

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // البحث عن جميع اللاعبين في المشهد (يفترض أن لديهم Tag "Player")
        allPlayers = GameObject.FindGameObjectsWithTag("Player").Select(g => g.GetComponent<Collider>()).ToArray();
    }

    void Update()
    {
        // 1. تحديد أقرب لاعب (فقط إذا لم يكن في حالة Panic أو Escape)
        if (currentState != State.Panic && currentState != State.Escape)
        {
            FindNearestPlayer();
        }

        // 2. إذا لم يكن هناك لاعب، لا تفعل شيئًا
        if (targetPlayer == null)
        {
            currentState = State.Patrol;
            Patrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

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
                if (distanceToPlayer <= attackDistance)
                    currentState = State.Attack;
                else if (distanceToPlayer > chaseDistance)
                    currentState = State.Patrol;
                break;

            case State.Attack:
                Attack();
                if (distanceToPlayer > attackDistance)
                    currentState = State.Chase;
                break;

            case State.Panic:
                // لا شيء يحدث في Update، المنطق يتم في Coroutine
                break;

            case State.Escape:
                Escape();
                agent.speed = escapeSpeed;
                // شرط العودة إلى Patrol بعد الهروب لمسافة كافية
                if (agent.remainingDistance < 1f && !agent.pathPending)
                {
                    currentState = State.Patrol;
                }
                break;
        }
    }

    // =================================================================
    // دوال الحالات (States)
    // =================================================================

    void Patrol()
    {
        agent.isStopped = false;
        if (patrolPoints.Length == 0) return;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.SetDestination(patrolPoints[currentIndex].position);
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(targetPlayer.position);
    }

    void Attack()
    {
        agent.isStopped = true;
        transform.LookAt(targetPlayer.position);
        // أضف منطق الهجوم هنا
    }

    // =================================================================
    // منطق الهروب والعمى
    // =================================================================

    // الدالة العامة التي يستدعيها الـ Raycast
    public void TriggerBlindness()
    {
        // نتأكد أن العدو ليس بالفعل في حالة Panic أو Escape
        if (currentState == State.Panic || currentState == State.Escape) return;

        // إيقاف أي Coroutine سابقة وبدء Coroutine جديدة
        StopAllCoroutines();
        StartCoroutine(PanicRoutine());
    }

    IEnumerator PanicRoutine()
    {
        currentState = State.Panic;
        agent.isStopped = true;
        
        // 1. تفعيل تأثير الانفجار/العمى
        if (blindParticlePrefab != null)
        {
            // ننشئ التأثير في موقع العدو
            Instantiate(blindParticlePrefab, transform.position, Quaternion.identity);
        }
        
        // 2. انتظار مدة العمى
        yield return new WaitForSeconds(panicDuration);
        
        // 3. الانتقال إلى حالة الهروب
        currentState = State.Escape;
        agent.isStopped = false;
    }

    void Escape()
    {
        // 1. إيجاد أقرب نقطة Patrol للهروب إليها
        Transform nearestPatrolPoint = GetNearestPatrolPoint();
        
        if (nearestPatrolPoint != null)
        {
            // 2. تعيين الوجهة إلى نقطة الهروب
            agent.SetDestination(nearestPatrolPoint.position);
        }
        else
        {
            // إذا لم تكن هناك نقاط Patrol، يهرب بعيدًا عن اللاعب
            Vector3 runDirection = transform.position - targetPlayer.position;
            Vector3 safePosition = transform.position + runDirection.normalized * 10f;
            agent.SetDestination(safePosition);
        }
    }

    // =================================================================
    // دوال المساعدة (Helper Functions)
    // =================================================================

    void FindNearestPlayer()
    {
        // ... (منطق البحث عن أقرب لاعب) ...
        Transform bestTarget = null;
        float closestDistanceSqr = chaseDistance * chaseDistance;
        Vector3 currentPosition = transform.position;

        // يجب أن يكون اللاعبون لديهم Tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject playerObject in players)
        {
            Vector3 directionToTarget = playerObject.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = playerObject.transform;
            }
        }

        targetPlayer = bestTarget;
    }

    Transform GetNearestPatrolPoint()
    {
        Transform nearestPoint = null;
        float minDistance = float.MaxValue;

        foreach (Transform point in patrolPoints)
        {
            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }
        return nearestPoint;
    }

    // =================================================================
    // Gizmos
    // =================================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}