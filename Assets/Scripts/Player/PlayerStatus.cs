using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public PlayerData playerData;
    public HeartUIController uiController;
    public PlayerHealth playerHealth;

    [Header("현재 상태")]
    public float currentHP;
    public float maxHP; // 근데 이거 이미 PlayerData 쪽에서 제어하는데 여기에 또 써도 되나???
    public float attackPower;
    public float moveSpeed;
    public float defense;
    public float critChance;
    public float critMultiplier;

    [Header("공포 게이지 관련")]
    public float fearGauge = 0f;
    public bool isTransformed = false;

    [Header("추가 필드 (디버프용)")]
    public bool canHeal = true;
    public bool isLethalOnHit = false;
    public float lethalDamagePercent = 1f;
    public bool canSwitchCharacter = true;
    public float skillCooldownMultiplier = 1f;

    public float extraHP = 0f;

    void Start()
    {
        if (playerData != null)
            InitializeFromData();
        else
            Debug.LogError("PlayerData가 할당되지 않았습니다.");
    }

    void InitializeFromData()
    {
        currentHP = playerData.maxHP; // 봐 여기 이미 playerData 참조 중이잖아.
        attackPower = playerData.attackPower;
        defense = playerData.defense;
        critChance = playerData.critChance;
        critMultiplier = playerData.critMultiplier;
        moveSpeed = playerData.moveSpeed;
    }

    public void ApplyDamage(float amount)
    {
        playerHealth.TakeDamage(amount);
    }

    public void RestoreHealth(float amount)
    {
        playerHealth.Heal(amount);
    }

    public void DisableEquipmentEffects()
    {
        Debug.Log("장비 효과 제거됨");
        // 여기에 장비 효과 비활성화 로직 작성
    }

    public void OnBossDefeated()
    {
        float hpIncreaseAmount = 1f; // 보스 하나당 증가할 최대 체력량- 이건 다시 생각해보자.
        playerData.IncreaseMaxHP(hpIncreaseAmount);
        uiController.UpdateHearts(playerHealth.currentHealth, playerData.currentMaxHP);
    }

    public void OnMaxHpItemPurchased()
    {
        float hpIncreaseAmount = 30f; // 상점 아이템 효과
        playerData.IncreaseMaxHP(hpIncreaseAmount);
        uiController.UpdateHearts(playerHealth.currentHealth, playerData.currentMaxHP);
    }
}


