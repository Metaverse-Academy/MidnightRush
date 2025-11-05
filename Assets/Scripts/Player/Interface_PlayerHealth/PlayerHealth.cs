using UnityEngine;
public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int health = 100;
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Player took " + damage + " damage. Remaining health: " + health);
        if (health <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        Debug.Log("Player died.");
        Destroy(gameObject);
    }
}