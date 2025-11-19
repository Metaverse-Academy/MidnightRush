using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private GameManager gameManager;
    void Start()
    {
        currentHealth = maxHealth;

        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene.");
        }
    }
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Remaining health: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");

        if (gameManager != null)
        {
            gameManager.CheckGameStatus();
        }

        gameObject.SetActive(false);
    }
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
