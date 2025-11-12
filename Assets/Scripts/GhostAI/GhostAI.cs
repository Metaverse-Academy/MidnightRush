using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public Transform player;               // اسحب لاعبك هنا من الـInspector
    public NavMeshAgent agent;
    public float chaseDistance = 20f;      // أقصى مسافة يبدأ فيها الشبح المطاردة
    public float stopDistance = 1.5f;      // مسافة للتوقف بحيث لا يصطدم
    public float loseSightTime = 3f;       // وقت يحتاجه قبل التراجع لو اللاعب صار بمضيء
    public bool alwaysChaseInDark = true;  // لو false تستخدم شروط إضافية

    // نفترض أن اللاعب لديه سكربت PlayerLightState يحتوي boolean isInDark
    private PlayerLightState playerLightState;
    private float darkLoseTimer = 0f;
    private enum State { Idle, Chasing, Searching }
    private State state = State.Idle;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) Debug.LogError("GhostAI: player not assigned!");
        playerLightState = player.GetComponent<PlayerLightState>();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool playerInDark = playerLightState != null ? playerLightState.isInDark : false;

        // حالة البدء: إذا اللاعب مظلم وفي نطاق يبدأ المطاردة
        if (playerInDark && dist <= chaseDistance)
        {
            darkLoseTimer = 0f;
            state = State.Chasing;
        }
        else
        {
            // لو كان يطارد وبعدين اللاعب صار بمضيء -> بدء العد للانسحاب
            if (state == State.Chasing)
            {
                darkLoseTimer += Time.deltaTime;
                if (darkLoseTimer >= loseSightTime)
                {
                    state = State.Searching; // ممكن يفتش شوي ثم يرجع Idle
                    StartCoroutine(SearchThenIdle());
                }
            }
        }

        // نفّذ السلوك وفق الحالة
        if (state == State.Chasing)
        {
            agent.isStopped = false;
            agent.stoppingDistance = stopDistance;
            agent.SetDestination(player.position);
            // هنا تقدر تشغل صوت خطوات أو أنيميشن
        }
        else // Idle / Searching
        {
            agent.isStopped = true;
            // حركة دورية بسيطة او وقوف
        }
    }

    System.Collections.IEnumerator SearchThenIdle()
    {
        float searchDuration = 4f; // يدوّر شوية
        float t = 0f;
        while (t < searchDuration)
        {
            t += Time.deltaTime;
            // ممكن تضيف حركة عشوائية قصيرة حول آخر موقع معروف
            yield return null;
        }
        state = State.Idle;
    }
}