using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Linq;

public class GhostAI : MonoBehaviour
{
    [Header("AI Behavior")]
    public NavMeshAgent agent;
    public string playerTag = "Player";
    [Tooltip("المسافة التي يتوقف عندها الشبح عن اللاعب ليمسكه")]
    public float catchDistance = 1.5f;
    [Tooltip("سرعة تلاشي الشبح عند تعرضه للضوء")]
    public float fadeOutDuration = 1.5f;

    [Header("Respawn")]
    [Tooltip("الوقت الذي يستغرقه الشبح للظهور مرة أخرى")]
    public float respawnTime = 10f;
    [Tooltip("نقاط الرسبنة المظلمة فقط")]
    public Transform[] spawnPoints;

    private Transform currentTarget;
    private bool isVanishing = false; // لمنع تنفيذ الأوامر أثناء التلاشي

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = catchDistance;
    }

    void Start()
    {
        // ابدأ الشبح في حالة خمول ومخفي
        gameObject.SetActive(false);
        StartCoroutine(RespawnRoutine());
    }

    void Update()
    {
        // إذا كان الشبح يتلاشى أو ليس لديه هدف، لا تفعل شيئًا
        if (isVanishing || currentTarget == null)
        {
            agent.isStopped = true;
            return;
        }

        // طارد الهدف الحالي باستمرار
        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);

        // تحقق من المسافة لمسك اللاعب
        if (Vector3.Distance(transform.position, currentTarget.position) <= catchDistance)
        {
            CatchPlayer(currentTarget);
        }
    }

    // --- نظام البحث عن الهدف ---
    private void FindBestTarget()
    {
        // ابحث عن كل اللاعبين
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        Transform bestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var playerObject in players)
        {
            // تحقق مما إذا كان اللاعب في الظلام
            var lightState = playerObject.GetComponent<PlayerLightState>();
            if (lightState != null && lightState.isInDark)
            {
                float distance = Vector3.Distance(transform.position, playerObject.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestTarget = playerObject.transform;
                }
            }
        }
        currentTarget = bestTarget;
    }

    // --- نظام الخوف من الضوء ---

    // يتم استدعاؤها من سكربت الفلاش لايت
    public void VanishFromFlashlight()
    {
        Debug.Log("Ghost is hit by flashlight! Vanishing...");
        StartCoroutine(VanishAndRespawn());
    }

    // يتم استدعاؤها من سكربت منطقة الضوء
    public void VanishFromLitRoom()
    {
        Debug.Log("Ghost is trapped in a lit room! Vanishing...");
        StartCoroutine(VanishAndRespawn());
    }

    // --- نظام الرسبنة والتلاشي ---
    private IEnumerator VanishAndRespawn()
    {
        if (isVanishing) yield break; // لا تتلاشى إذا كنت تتلاشى بالفعل

        isVanishing = true;
        agent.isStopped = true;
        agent.ResetPath();

        // يمكنك إضافة أنيميشن أو صوت تلاشي هنا

        // منطق التلاشي (يمكنك استخدام نفس الكود السابق للتلاشي البصري)
        float timer = 0;
        while (timer < fadeOutDuration)
        {
            // قم بتخفيض شفافية الشبح هنا
            timer += Time.deltaTime;
            yield return null;
        }

        // إخفاء الشبح وبدء مؤقت الرسبنة
        gameObject.SetActive(false);
        isVanishing = false;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);

        // ابحث عن نقطة رسبنة صالحة (مظلمة)
        Transform spawnPoint = GetRandomDarkSpawnPoint();
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            gameObject.SetActive(true);
            agent.Warp(transform.position); // تأكد من الالتصاق بالـ NavMesh
            Debug.Log("Ghost has respawned.");
            // ابدأ بالبحث عن هدف بعد الرسبنة
            FindBestTarget();
        }
        else
        {
            // إذا كانت كل الأماكن مضاءة، حاول مرة أخرى لاحقًا
            Debug.Log("All spawn points are lit. Retrying respawn later...");
            StartCoroutine(RespawnRoutine());
        }
    }

    private Transform GetRandomDarkSpawnPoint()
    {
        // قم بخلط نقاط الرسبنة للعشوائية
        var shuffledPoints = spawnPoints.OrderBy(x => Random.value).ToList();

        foreach (var point in shuffledPoints)
        {
            // تحقق مما إذا كانت النقطة داخل أي منطقة مضاءة
            Collider[] colliders = Physics.OverlapSphere(point.position, 0.5f);
            bool inLight = colliders.Any(c => c.GetComponent<LightRoomTrigger>() != null && c.GetComponent<LightRoomTrigger>().IsLightOn());
            
            if (!inLight)
            {
                return point; // وجدنا نقطة مظلمة
            }
        }
        return null; // لم نجد أي نقطة مظلمة
    }

    private void CatchPlayer(Transform player)
    {
        Debug.Log("GAME OVER! Ghost caught " + player.name);
        // ضع هنا منطق نهاية اللعبة
        // مثال: Time.timeScale = 0; أو SceneManager.LoadScene("GameOver");
    }
}
