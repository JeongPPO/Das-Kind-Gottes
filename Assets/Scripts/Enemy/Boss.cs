using UnityEngine;

public class Boss : MonoBehaviour, IDamageable
{
    public float maxHP = 500f;
    public float currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        BossUI.Instance.UpdateHPBar(currentHP / maxHP);

        if (currentHP <= 0)
            Die();
    }

    public Vector3 GetPosition() => transform.position;

    private void Die()
    {
        Debug.Log("보스 사망!");
    }
}
