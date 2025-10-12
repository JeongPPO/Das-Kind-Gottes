using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public PlayerData playerData;
    public HeartUIController uiController;
    public PlayerHealth playerHealth;

    [Header("캐릭터 정보")]
    public int currentCharacterIndex; // 1 = Player1, 2 = Player2, 3 = Player3

    [Header("현재 상태")]
    public float moveSpeed;         // 현재 이동 속도
    [HideInInspector] public float originalSpeed; // 원래 속도 저장
    
    public float attackPower;
    public float defense;
    public float critChance;
    public float critMultiplier;

    [Header("공포 게이지 관련")]
    public float fearGauge = 0f;
    public bool isTransformed = false;
    public float fearGaugeChargeRate = 1.0f;

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
       originalSpeed = moveSpeed; // 이동 속도 초기화
    }

    public void SetCurrentCharacter(int index) // 이거 진짜 사용되고 있는 걸까??
    {
        currentCharacterIndex = index;
    }

    public void InitializeFromData()
    {
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


