using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damageAmount = 25;
    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
            Debug.Log("Player dealt " + damageAmount + " damage to " + collision.gameObject.name);
        }
    }
}