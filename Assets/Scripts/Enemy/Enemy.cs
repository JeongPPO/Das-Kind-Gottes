using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyData enemyData;
    private float currentHealth;

    void Start()
    {
        currentHealth = enemyData.maxHealth;
    }

    public float damageThresholdForMonsterMode =>
        enemyData != null ? enemyData.damageThresholdForMonsterMode : float.MaxValue;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    public Vector3 GetPosition() => transform.position;

    void Die()
    {
        Debug.Log($"{name} 처치됨!");
        Destroy(gameObject);
    }
}