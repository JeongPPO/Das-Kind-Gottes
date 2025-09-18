using UnityEngine;

public class Boss : MonoBehaviour
{
    public static Boss Instance;
    public float maxHP = 500f;
    public float currentHP;

    private void Awake()
    {
        Instance = this;
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        BossUI.Instance.UpdateHPBar(currentHP / maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("보스 사망!");
        // 애니메이션, 클리어 연출 등
    }
}

