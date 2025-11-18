using UnityEngine;
using System.Collections;

public class TheEnemyAI : MonoBehaviour
{
    public enum State { Spawn, Chase, Panic, Escape }
    public State currentState;

    [Header("Target & Movement")]
    public Transform[] players;
    [Tooltip("the movement speed of the enemy")]
    public float speed = 4f;
    [Tooltip("the maximum distance the enemy can move away from its spawn point before returning")]
    public float maxChaseDistance = 20f;

    [Header("Respawn Settings")]
    [Tooltip("the spawn points where the enemy can appear")]
    public Transform[] spawnPoints;
    [Tooltip("the time the enemy hides when panicking (e.g., when exposed to light)")]
    public float panicHideTime = 3f;
    [Tooltip("the time the enemy hides when escaping (e.g., when trapped)")]
    public float escapeRespawnTime = 10f;

    [Header("Visuals & Audio")]
    [Tooltip("the GameObject representing the enemy's body to hide and show")]
    public GameObject enemyBody;
    [Tooltip("the AudioSource component to play sound effects")]
    public AudioClip chaseSound;
    public AudioClip panicSound;
    public AudioClip escapeSound;
    private Transform targetPlayer;
    private Vector3 homePosition;
    private bool isRoutineActive = false;

    [Header("Effects")]
    public GameObject blindParticlePrefab;

    [Header("Audio Clips")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ChaseSound; // Sound to play when the enemy is chasing
    [SerializeField] private AudioClip AttackSound; // Sound to play when the enemy is
    [SerializeField] private AudioClip EscapeSound;
    [SerializeField] private AudioClip DisapearSound; // AudioSource component to play the sound


    void OnEnable()
    {
        currentState = State.Spawn;
        Respawn();
    }
    void Update()
    {
        if (isRoutineActive) return;

        switch (currentState)
        {
            case State.Spawn:
                PickClosestPlayer();
                if (targetPlayer != null)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                ChasePlayer();
                break;

            case State.Escape:
                OnLightExposed();
                break;

        }
    }
    void PickClosestPlayer()
    {
        if (players == null || players.Length == 0) return;

        float minDistance = float.MaxValue;
        Transform closestPlayer = null;

        foreach (Transform p in players)
        {
            if (p == null) continue;
            float distance = Vector3.Distance(transform.position, p.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = p;
            }
        }
        targetPlayer = closestPlayer;
    }

    void ChasePlayer()
    {
        if (targetPlayer == null)
        {
            PickClosestPlayer();
            return;
        }

        if (maxChaseDistance > 0 && Vector3.Distance(homePosition, transform.position) > maxChaseDistance)
        {
            Debug.Log("The enemy has moved too far away and will respawn.");
            currentState = State.Spawn;
            Respawn();
            return;
        }

        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        transform.position += new Vector3(direction.x, direction.y, direction.z) * speed * Time.deltaTime;

        if (audioSource != null && chaseSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = chaseSound;
            audioSource.Play();
        }
    }

    void Respawn()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points defined for the enemy!");
            return;
        }

        int randIndex = Random.Range(0, spawnPoints.Length);
        transform.position = spawnPoints[randIndex].position;
        homePosition = transform.position;

        Debug.Log("The enemy spawned at: " + spawnPoints[randIndex].name);

        if (enemyBody != null) enemyBody.SetActive(true);

        PickClosestPlayer();
    }
    public void OnLightExposed()
    {
        // Debug.Log("The enemy was exposed to light! Starting escape state.");
        Debug.Log($"The Current State is: {currentState}");
        currentState = State.Escape;
        StartCoroutine(EscapeRoutine());
    }
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            currentState = State.Escape;
            Debug.Log("The enemy was exposed to light! Starting panic state.");
            StartCoroutine(EscapeRoutine());
        }
        // else if (other.CompareTag("Trap"))
        // {
        //     Debug.Log("The enemy entered a trap! Starting escape state.");
        //     StartCoroutine(EscapeRoutine());
        // }
    }

    IEnumerator PanicRoutine()
    {
        //if (isRoutineActive) yield break;
        isRoutineActive = true;
        currentState = State.Panic;

        if (audioSource != null && panicSound != null) audioSource.PlayOneShot(panicSound);
        if (blindParticlePrefab != null)
        {
            GameObject blindEffect = Instantiate(blindParticlePrefab, transform.position, Quaternion.identity);
            blindEffect.transform.SetParent(transform);
        }
        if (enemyBody != null) enemyBody.SetActive(false);

        yield return new WaitForSeconds(panicHideTime);

        Respawn();
        currentState = State.Spawn;
        isRoutineActive = false;
    }

    IEnumerator EscapeRoutine()
    {
        //if (isRoutineActive) yield break;
        isRoutineActive = true;
        currentState = State.Escape;

        if (audioSource != null && escapeSound != null) audioSource.PlayOneShot(escapeSound);

        StartCoroutine(FadeOut());

        yield return new WaitForSeconds(escapeRespawnTime);

        isRoutineActive = false;
        Respawn();
        currentState = State.Spawn;
    }

    IEnumerator FadeOut()
    {
        Renderer bodyRenderer = enemyBody.GetComponent<Renderer>();
        if (bodyRenderer == null)
        {
            Debug.LogWarning("No Renderer component found on the enemy's body for fading.");
            enemyBody.SetActive(false);
            yield break;
        }

        Material mat = bodyRenderer.material;
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;

        float duration = 0.5f;
        float timer = 0;
        Color startColor = mat.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        mat.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        enemyBody.SetActive(false);
    }

}