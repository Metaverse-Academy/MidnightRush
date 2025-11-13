using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TheEnemyAI : MonoBehaviour
{
    private enum State { Chase, Escape, Scared }
    private State currentState = State.Chase;
    public Transform[] childTargets;
    public float chaseSpeed = 3f;
    // public float scaredSpeed = 2f;
    private NavMeshAgent agent;
    private Transform targetChild;
    public Light[] roomLights;
    private bool isInLitRoom = false;
    private bool isScaredByFlash = false;
    private bool isEscaping = false;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
    }
    void Update()
    {
        if (isEscaping) return;

        FindNearestChild();
        isInLitRoom = CheckIfInLitRoom();

        if (isInLitRoom || isScaredByFlash)
        {
            StartCoroutine(EscapeAndFadeOut());
        }
        else if (currentState == State.Chase)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            ChaseChild();
        }
    }
    private IEnumerator EscapeAndFadeOut()
    {
        isEscaping = true;
        agent.isStopped = true;
        currentState = State.Escape;
        Debug.Log("The ghost is escaping!");

        float fadeTime = 1.5f;
        float elapsedTime = 0f;
        Renderer ghostRenderer = GetComponent<Renderer>();
        Color initialColor = ghostRenderer.material.color;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            ghostRenderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(10f);

        Debug.Log("The ghost has returned!");
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
            ghostRenderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        ghostRenderer.material.color = initialColor;

        isScaredByFlash = false;
        isEscaping = false;
        currentState = State.Chase;
        agent.isStopped = false;
    }
    public void TriggerFlashLight()
    {
        isScaredByFlash = true;
    }
    void FindNearestChild()
    {
        if (childTargets.Length == 0) return;
        float shortestDistance = Mathf.Infinity;
        foreach (Transform child in childTargets)
        {
            float distance = Vector3.Distance(transform.position, child.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                targetChild = child;
            }
        }
    }
    bool CheckIfInLitRoom()
    {
        foreach (Light light in roomLights)
        {
            if (light.enabled && Vector3.Distance(transform.position, light.transform.position) < 5f)
            {
                return true;
            }
        }
        return false;
    }
    void ChaseChild()
    {
        if (targetChild != null)
        {
            agent.SetDestination(targetChild.position);
        }

        if (targetChild != null && Vector3.Distance(transform.position, targetChild.position) < 2f)
        {
            Debug.Log("Game Over! Ghost Caught the Child.");
        }
    }
}
