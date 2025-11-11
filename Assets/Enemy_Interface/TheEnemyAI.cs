using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TheEnemyAI : MonoBehaviour
{
    // تعريف الحالات التي يمكن أن يكون فيها العدو
    private enum State { Spawning, Chase, Attack, Panic, Escape }
    [SerializeField] private State currentState = State.Spawning;

    [Header("Spawning Settings")]
    [Tooltip("نقاط الرسبنة التي يمكن أن يظهر فيها العدو")]
    [SerializeField] private Transform[] spawnPoints;
    [Tooltip("الوقت بالثواني قبل أن يعود العدو للظهور بعد أن يتلاشى")]
    [SerializeField] private float respawnTime = 10f;

    [Header("Detection Settings")]
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float escapeDistance = 15f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRate = 1.5f;
    private float nextAttackTime = 0f;

    [Header("Panic & Escape Settings")]
    [SerializeField] private float panicDuration = 2f;
    [SerializeField] private GameObject blindParticlePrefab;
    [Tooltip("سرعة تلاشي العدو عند الهروب (مثلاً 2 ثانية)")]
    [SerializeField] private float fadeOutDuration = 2f;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private Renderer objectRenderer; // للتحكم في شفافية العدو
    private Collider objectCollider; // لتعطيل الاصطدام عند التلاشي

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        objectRenderer = GetComponentInChildren<Renderer>(); // يحصل على أول Renderer في العدو أو أبنائه
        objectCollider = GetComponent<Collider>();

        // ابدأ العدو في حالة "الظهور" لكي لا يفعل شيئاً حتى يرسبن
        currentState = State.Spawning;
        // اجعل العدو غير مرئي ومُعطل في البداية
        SetEnemyActive(false);
    }

    void Start()
    {
        // ابدأ أول عملية رسبنة
        StartCoroutine(RespawnRoutine());
    }

    void Update()
    {
        // --- التعديل الرئيسي هنا ---
        // إذا كان العدو في أي حالة لا تتطلب الحركة، اخرج من الدالة فوراً.
        // هذا يمنع استدعاء Chase() أو Attack() عندما يكون الـ NavMeshAgent معطلاً.
        if (currentState == State.Spawning || currentState == State.Panic || currentState == State.Escape)
        {
            return;
        }
        // ---------------------------

        FindNearestPlayer();
        float distanceToPlayer = targetPlayer ? Vector3.Distance(transform.position, targetPlayer.position) : float.MaxValue;

        // هذا الجزء الآن آمن لأننا تجاوزنا الحالات الخاصة في الأعلى
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
            // إذا فقد اللاعب، يبدأ العدو في التلاشي
            Debug.Log("Player lost, enemy will fade out and respawn.");
            currentState = State.Escape; // نغير الحالة إلى Escape لبدء التلاشي
            StartCoroutine(FadeOutAndRespawn());
            return; // اخرج من الدالة بعد بدء التلاشي
        }

        // تنفيذ السلوك بناءً على الحالة
        switch (currentState)
        {
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    // --- نظام الرسبنة الجديد ---
    IEnumerator RespawnRoutine()
    {
        Debug.Log("Waiting to respawn for " + respawnTime + " seconds...");
        yield return new WaitForSeconds(respawnTime);

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned! Cannot respawn enemy.");
            yield break; // اخرج من الكوروتين إذا لم تكن هناك نقاط رسبنة
        }

        // اختر نقطة رسبنة عشوائية
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // --- التعديل الرئيسي هنا ---
        // 1. انقل المجسم (transform) أولاً إلى موقع نقطة الرسبنة.
        //    هذا يضمن أن العدو في الموقع الصحيح قبل تفعيل الـ agent.
        transform.position = spawnPoint.position;

        // 2. الآن قم بتفعيل العدو (بما في ذلك الـ NavMeshAgent).
        //    بما أن العدو الآن قريب من الـ NavMesh، يجب أن تتم عملية التفعيل بنجاح.
        SetEnemyActive(true);

        // 3. (خطوة أمان إضافية) استخدم Warp للتأكد من أن الـ agent "يستقر" تمامًا على الـ NavMesh.
        //    هذا مفيد إذا كانت نقطة الرسبنة فوق السطح قليلاً.
        agent.Warp(spawnPoint.position);
        // ---------------------------

        Debug.Log("Enemy has respawned at " + spawnPoint.name);

        // ابدأ في البحث عن اللاعب
        currentState = State.Chase;
    }


    // --- نظام المطاردة والهجوم (بدون تغيير) ---
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

    // --- نظام الذعر والهروب المحدث ---
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

        // بعد الذعر، ابدأ في الهروب والتلاشي
        currentState = State.Escape;
        Debug.Log("The Enemy is Escaping and fading out!");
        StartCoroutine(EscapeAndFadeOut());
    }

    // --- دالة الهروب والتلاشي الجديدة ---
    IEnumerator EscapeAndFadeOut()
    {
        // 1. حساب نقطة الهروب الصحيحة (مرة واحدة فقط)
        Vector3 escapeDirection = (transform.position - targetPlayer.position).normalized;
        // تأكد من أن النقطة على الـ NavMesh
        NavMeshHit hit;
        Vector3 escapePoint = transform.position + escapeDirection * escapeDistance;
        if (NavMesh.SamplePosition(escapePoint, out hit, 5f, NavMesh.AllAreas))
        {
            escapePoint = hit.position;
        }

        agent.isStopped = false;
        agent.SetDestination(escapePoint);

        // 2. ابدأ في التلاشي أثناء الحركة
        StartCoroutine(FadeOutAndRespawn());

        // انتظر حتى يصل العدو إلى وجهته أو يقترب منها
        while (agent.pathPending || agent.remainingDistance > 1.5f)
        {
            yield return null; // انتظر الإطار التالي
        }
    }

    // --- دالة التلاشي وإعادة الرسبنة ---
    IEnumerator FadeOutAndRespawn()
    {
        // إذا كان هناك Renderer، قم بتغيير المادة إلى شفافة
        Material originalMaterial = objectRenderer.material;
        Material tempMaterial = new Material(originalMaterial);
        tempMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        tempMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        tempMaterial.SetInt("_ZWrite", 0);
        tempMaterial.EnableKeyword("_ALPHABLEND_ON");
        tempMaterial.renderQueue = 3000;
        objectRenderer.material = tempMaterial;

        float timer = 0;
        Color startColor = tempMaterial.color;

        // حلقة التلاشي
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            tempMaterial.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // بعد التلاشي الكامل، قم بإلغاء تنشيط العدو
        SetEnemyActive(false);
        objectRenderer.material = originalMaterial; // أعد المادة الأصلية

        // ابدأ روتين الرسبنة من جديد
        StartCoroutine(RespawnRoutine());
    }

    // --- دوال مساعدة ---
    void SetEnemyActive(bool isActive)
    {
        // تفعيل/إلغاء تفعيل المكونات الرئيسية للعدو
        if (objectRenderer) objectRenderer.enabled = isActive;
        if (objectCollider) objectCollider.enabled = isActive;
        if (agent) agent.enabled = isActive;

        // إعادة لون المادة إلى طبيعته عند التفعيل
        if (isActive && objectRenderer != null)
        {
            Color c = objectRenderer.material.color;
            objectRenderer.material.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    void FindNearestPlayer()
    {
        // ... (هذه الدالة تعمل بشكل جيد، لا حاجة للتغيير)
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
        targetPlayer = nearestPlayer;
    }

    void OnDrawGizmosSelected()
    {
        // ... (لا حاجة للتغيير)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
