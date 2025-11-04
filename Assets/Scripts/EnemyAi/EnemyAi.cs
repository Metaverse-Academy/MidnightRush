using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class EnemyAi : MonoBehaviour
{
    public enum State { Dormant, Stalking, Chasing, Repelled, Cooldown }

    [Header("Spawning")]
    [Tooltip("If empty, we will auto-find scene objects tagged 'DarkSpawn'.")]
    public Transform[] darkSpawns;
    [Tooltip("Seconds she stays gone after being hit by light.")]
    public float respawnDelay = 3f;

    [Header("Movement")]
    public bool useNavMesh = false;
    public float stalkSpeed = 1.6f;

    [Header("FX Hooks (optional)")]
    public ParticleSystem vanishFX;
    public AudioSource vanishSFX;

    [Header("Stalking Tunables")]
    [SerializeField] private float detectRange = 20f;
    [SerializeField] private float minDarknessToAdvance = 0.25f; // won’t step into bright area
    [SerializeField] private float retargetEvery = 0.5f;

    [Header("Debug")]
    public bool showSpawnGizmos = true;

    // internals
    private State state = State.Dormant;
    private Renderer[] rends;
    private Collider bodyCol;
    private NavMeshAgent agent;
    private int lightZoneCount = 0;
    private bool isHidden = false;
    private float retargetTimer;

    private Transform[] players;
    private DarknessZone[] zones;

    [SerializeField] private float chaseSpeed = 4.0f;
    [SerializeField] private float chaseBurstTime = 2.5f;
    [SerializeField] private float chaseCooldownTime = 4f;
    private float chaseTimer;
    private bool chaseOnCooldown;


    void Awake()
    {
        rends   = GetComponentsInChildren<Renderer>(true);
        bodyCol = GetComponent<Collider>();

        // Ensure we receive trigger events vs. LightZone triggers
        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;

        if (useNavMesh)
        {
            agent = GetComponent<NavMeshAgent>();
            if (!agent) agent = gameObject.AddComponent<NavMeshAgent>();
            agent.speed = stalkSpeed;
            agent.acceleration = 20f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0f;
        }

        // If no spawns assigned, auto-find by tag
        if (darkSpawns == null || darkSpawns.Length == 0)
        {
            var gos = GameObject.FindGameObjectsWithTag("DarkSpawn");
            darkSpawns = new Transform[gos.Length];
            for (int i = 0; i < gos.Length; i++) darkSpawns[i] = gos[i].transform;
        }

        // >>> MOVE THESE HERE so they're ready before first respawn <<<
        players = GameObject.FindGameObjectsWithTag("Player")
        .Select(go => go.transform)
        .ToArray();
        
        zones = FindObjectsByType<DarknessZone>(FindObjectsSortMode.None);
    }

    void Start()
    {
        // First appearance: spawn hidden -> appear
        StartCoroutine(RespawnRoutine());
    }

    void Update()
    {
        // test key: pretend flashlight hit
        if (Input.GetKeyDown(KeyCode.K)) OnFlashlightHit();
    }

   void FixedUpdate()
{
    // --- STALKING: slow, periodic retarget ---
    if (useNavMesh && agent && state == State.Stalking)
    {
        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetEvery;

            var target = PickTargetPlayer();
            if (target != null)
            {
                float darkAtDest = SampleDarknessAt(target.position);
                if (darkAtDest >= minDarknessToAdvance)
                {
                    agent.speed = stalkSpeed;
                    agent.SetDestination(target.position);
                }
                else
                {
                    // hold at edge of darkness
                    agent.ResetPath();
                }
            }

            // Try to flip into a short chase burst if conditions are dangerous
            if (ShouldChaseBurst())
            {
                state = State.Chasing;
                chaseTimer = chaseBurstTime;
            }
        }
    }

    // --- CHASING: brief burst toward target ---
    if (useNavMesh && agent && state == State.Chasing)
    {
        if (chaseTimer > 0f)
        {
            chaseTimer -= Time.fixedDeltaTime;
            var target = PickTargetPlayer();
            if (target)
            {
                agent.speed = chaseSpeed;
                agent.SetDestination(target.position);
            }
            else
            {
                agent.ResetPath();
            }

            if (chaseTimer <= 0f)
            {
                chaseOnCooldown = true;
                StartCoroutine(ChaseCooldown());
                state = State.Stalking;
            }
        }
    }
}

// Cooldown after a chase burst so she doesn't chain-chase constantly
    private IEnumerator ChaseCooldown()
    {
    yield return new WaitForSeconds(chaseCooldownTime);
    chaseOnCooldown = false;
    }

// Simple gate: chase when protection is weak and target area is dark
    private bool ShouldChaseBurst()
    {
    if (chaseOnCooldown || state != State.Stalking) return false;

    bool danger = (GameDanger.I == null) || (!GameDanger.I.flashlightOn || GameDanger.I.lampCharge < 0.25f);

    var t = PickTargetPlayer();
    if (!t) return false;

    float dark = SampleDarknessAt(t.position);
    return danger && dark >= 0.5f;
    }


    public void OnFlashlightHit()
    {
        if (state == State.Repelled || state == State.Cooldown) return;
        Repel();
    }

    // Enter/exit a room light trigger collider tagged "LightZone"
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LightZone"))
        {
            lightZoneCount++;
            Repel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LightZone"))
        {
            lightZoneCount = Mathf.Max(0, lightZoneCount - 1);
        }
    }

    // ===== CORE BEHAVIOUR =====

    private void Repel()
    {
        if (state == State.Repelled || state == State.Cooldown) return;

        state = State.Repelled;

        if (vanishSFX) vanishSFX.Play();
        if (vanishFX)  vanishFX.Play();

        SetActiveVisuals(false);
        SetActivePhysics(false);

        StopAllCoroutines();
        StartCoroutine(CooldownThenRespawn());
    }

    private IEnumerator CooldownThenRespawn()
    {
        state = State.Cooldown;
        yield return new WaitForSeconds(respawnDelay);
        yield return StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        var p = PickSpawn();
        if (p) transform.SetPositionAndRotation(p.position, p.rotation);

        // don’t appear inside an active light zone
        while (lightZoneCount > 0) yield return null;

        SetActivePhysics(true);
        SetActiveVisuals(true);

        state = State.Stalking;
        // Try to flip into a short chase
        if (ShouldChaseBurst())
        {
         state = State.Chasing;
        chaseTimer = chaseBurstTime;
        }

        
    }

    private Transform PickSpawn()
{
    if (darkSpawns == null || darkSpawns.Length == 0) return null;

    // Build weights based on darkness score
    var weights = darkSpawns.Select(s => Mathf.Max(0.1f, SampleDarknessAt(s.position))).ToArray();
    float total = weights.Sum();
    float pick = Random.value * total;
    float cumulative = 0f;

    for (int i = 0; i < darkSpawns.Length; i++)
    {
        cumulative += weights[i];
        if (pick <= cumulative)
            return darkSpawns[i];
    }

    // fallback (shouldn't happen)
    return darkSpawns[Random.Range(0, darkSpawns.Length)];
}


    private float SampleDarknessAt(Vector3 pos)
    {
        if (zones == null || zones.Length == 0) return 1f; // assume dark if no zones set yet
        float best = 0f;
        foreach (var z in zones) if (z) best = Mathf.Max(best, z.SampleDarkness(pos));
        return best;
    }

    private Transform PickTargetPlayer()
    {
        if (players == null || players.Length == 0) return null;

        Transform best = null;
        float bestScore = float.MinValue;

        foreach (var p in players)
        {
            if (!p) continue;
            float dist = Vector3.Distance(transform.position, p.position);
            if (dist > detectRange) continue;

            float dark = SampleDarknessAt(p.position); // 0..1
            float iso  = IsolationScore(p);             // 0..1
            float score = dark * 0.7f + iso * 0.3f;

            if (score > bestScore)
            {
                bestScore = score;
                best = p;
            }
        }
        return best;
    }

    private float IsolationScore(Transform p)
    {
        if (players == null || players.Length <= 1) return 1f; // single player = fully isolated

        float min = float.MaxValue;
        foreach (var q in players)
        {
            if (q == null || q == p) continue;
            min = Mathf.Min(min, Vector3.Distance(p.position, q.position));
        }
        // normalize 0..10m → 0..1
        return Mathf.Clamp01(min / 10f);
    }

    private void SetActiveVisuals(bool active)
    {
        isHidden = !active;
        foreach (var r in rends) if (r) r.enabled = active;
    }

    private void SetActivePhysics(bool active)
    {
        if (bodyCol) bodyCol.enabled = active;
        if (agent)   agent.enabled   = active && useNavMesh;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showSpawnGizmos || darkSpawns == null) return;
        Gizmos.color = Color.magenta;
        foreach (var t in darkSpawns)
        {
            if (!t) continue;
            Gizmos.DrawWireSphere(t.position, 0.25f);
        }
    }
}
