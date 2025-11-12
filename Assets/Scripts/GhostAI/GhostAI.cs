using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GhostAI : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    private PlayerLightState playerLightState; // سيتم الحصول عليه تلقائياً

    [Header("AI Behavior")]
    public NavMeshAgent agent;
    [Tooltip("المسافة التي يبدأ عندها الشبح المطاردة")]
    public float chaseDistance = 20f;
    [Tooltip("المسافة التي يتوقف عندها الشبح عن اللاعب")]
    public float stoppingDistance = 1.5f;
    [Tooltip("الوقت الذي ينتظره الشبح قبل أن ينسحب بعد دخول اللاعب للضوء")]
    public float hesitationTime = 2f;

    // نظام الحالات ليكون أكثر وضوحاً
    private enum State { Idle, Chasing, Hesitating, Searching }
    private State currentState = State.Idle;

    private Coroutine hesitationCoroutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            // محاولة العثور على اللاعب تلقائياً إذا لم يتم تعيينه
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("GhostAI: Player not assigned and could not be found by tag 'Player'!");
                this.enabled = false; // عطّل السكربت إذا لم يتم العثور على اللاعب
                return;
            }
        }
        playerLightState = player.GetComponent<PlayerLightState>();
        if (playerLightState == null)
        {
            Debug.LogError("GhostAI: Player is missing the 'PlayerLightState' script!");
            this.enabled = false;
        }
    }

    void Start()
    {
        // تأكد من أن قيمة stoppingDistance في NavMeshAgent تتطابق مع ما نريده
        agent.stoppingDistance = this.stoppingDistance;
    }

    void Update()
    {
        // لا تفعل شيئاً إذا لم يكن هناك لاعب أو NavMeshAgent
        if (player == null || agent == null) return;

        // قم بتحديث الحالة الحالية بناءً على الظروف
        UpdateState();

        // نفّذ السلوك بناءً على الحالة المحدثة
        ExecuteStateBehavior();
    }

    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isPlayerInDark = playerLightState.isInDark;

        // إذا كان الشبح متردداً، لا تغير حالته حتى ينتهي التردد
        if (currentState == State.Hesitating)
        {
            return;
        }

        // الشرط الأساسي للمطاردة
        if (isPlayerInDark && distanceToPlayer <= chaseDistance)
        {
            // إذا كان متردداً أو يبحث، أوقف تلك العمليات وابدأ المطاردة فوراً
            if (hesitationCoroutine != null)
            {
                StopCoroutine(hesitationCoroutine);
                hesitationCoroutine = null;
            }
            currentState = State.Chasing;
        }
        // إذا كان يطارد واللاعب دخل الضوء
        else if (currentState == State.Chasing && !isPlayerInDark)
        {
            // ابدأ التردد (الانتظار قبل الانسحاب)
            currentState = State.Hesitating;
            hesitationCoroutine = StartCoroutine(HesitateThenSearch());
        }
        // إذا كان اللاعب بعيداً جداً أو في الضوء (ولم يكن يطارد)
        else if (currentState != State.Searching)
        {
            currentState = State.Idle;
        }
    }

    private void ExecuteStateBehavior()
    {
        switch (currentState)
        {
            case State.Chasing:
                // طارد اللاعب
                agent.isStopped = false;
                agent.SetDestination(player.position);
                // يمكنك إضافة أنيميشن أو صوت هنا
                break;

            case State.Idle:
            case State.Searching:
            case State.Hesitating:
                // توقف عن الحركة في جميع الحالات الأخرى
                agent.isStopped = true;
                // يمكنك إضافة أنيميشن الوقوف أو البحث هنا
                break;
        }
    }

    // كوروتين للتردد ثم البحث
    IEnumerator HesitateThenSearch()
    {
        // توقف وانتظر
        yield return new WaitForSeconds(hesitationTime);

        // بعد الانتظار، ابدأ البحث
        currentState = State.Searching;
        yield return new WaitForSeconds(4f); // مدة البحث

        // بعد البحث، عد إلى حالة الخمول
        currentState = State.Idle;
        hesitationCoroutine = null;
    }
}