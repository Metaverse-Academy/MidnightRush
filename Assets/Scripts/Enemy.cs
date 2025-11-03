using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Flash Reaction Settings")]
    public bool destroyOnFlash = false;
    public float disableDuration = 3f;

    private bool isDisabled = false;
    private Renderer enemyRenderer;
    private Collider enemyCollider;

    private void Awake()
    {
        enemyRenderer = GetComponentInChildren<Renderer>();
        enemyCollider = GetComponent<Collider>();
    }

    public void Fear()
    {
        if (isDisabled) return;

        if (destroyOnFlash)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(DisableTemporarily());
        }
    }

    private IEnumerator DisableTemporarily()
    {
        isDisabled = true;

        if (enemyRenderer != null) enemyRenderer.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        yield return new WaitForSeconds(disableDuration);

        if (enemyRenderer != null) enemyRenderer.enabled = true;
        if (enemyCollider != null) enemyCollider.enabled = true;

        isDisabled = false;
    }
}