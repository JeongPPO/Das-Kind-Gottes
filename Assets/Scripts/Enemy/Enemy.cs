using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    private float currentHealth;

    void Start()
    {
        currentHealth = enemyData.maxHealth;
    }

    public float damageThresholdForMonsterMode
    {
        get
        {
            if (enemyData != null)
                return enemyData.damageThresholdForMonsterMode;
            else
                return float.MaxValue; // 기본값 혹은 오류 처리
        }
    }
    // 필요시 enemyData의 다른 값들도 활용
}