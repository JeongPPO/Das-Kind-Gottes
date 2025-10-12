using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("연결 데이터")]
    public PlayerData playerData;
    public HeartUIController uiController;
    private PlayerStatus status;

    [Header("상태 값")]
    public float currentHealth;
    public bool isLethalOnHit = false;  // 즉사 여부
    public bool canHeal = true;         // 회복 가능 여부
    private float damageReduction = 0f; // 0~1 값 (0.1 = 10% 감소)

    IEnumerator Start()
    {
        yield return null; // 한 프레임 대기 (UI, 데이터 초기화 보장)
        InitializeHealth(true);
    }

    /// 체력 초기화
    public void InitializeHealth(bool resetToMax = false)
    {
        if (resetToMax || currentHealth <= 0)
            currentHealth = playerData.currentMaxHP;

        UpdateHealthUI();
    }

    /// 피해 처리
    public void TakeDamage(float amount)
    {
        if (isLethalOnHit)
        {
            Debug.Log("[PlayerHealth] 즉사 디버프 적용됨! 플레이어 사망");
            currentHealth = 0;
            Die();
            return;
        }

        // HealZone에서 적용되는 피해 감소율 반영
        float finalDamage = amount * (1f - damageReduction);

        currentHealth = Mathf.Max(0f, currentHealth - finalDamage);
        Debug.Log($"[PlayerHealth] 피해: {amount} → {finalDamage}, 현재 체력: {currentHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    /// 회복 처리
    public void Heal(float amount)
    {
        if (!canHeal)
        {
            Debug.Log("[PlayerHealth] 회복 불가 상태");
            return;
        }

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(playerData.currentMaxHP, currentHealth + amount);
        Debug.Log($"[PlayerHealth] 회복: {amount}, 체력 {oldHealth} → {currentHealth}");

        UpdateHealthUI();
    }

    /// 최대 체력 증가
    public void IncreaseMaxHealth(float amount)
    {
        playerData.IncreaseMaxHP(amount);
        Debug.Log($"[PlayerHealth] 최대 체력 증가: +{amount}, 새로운 최대 체력: {playerData.currentMaxHP}");

        currentHealth = Mathf.Min(currentHealth, playerData.currentMaxHP);
        UpdateHealthUI();
    }

    /// HealZone 등에서 버프를 통해 피해 감소율 설정
    /// <param name="reduction">0~1 사이 값</param>
    public void SetDamageReduction(float reduction)
    {
        damageReduction = Mathf.Clamp01(reduction);
        Debug.Log($"[PlayerHealth] 피해 감소율 적용: {damageReduction * 100f}%");
    }

    /// UI 갱신
    private void UpdateHealthUI()
    {
        if (uiController == null) return;
        uiController.SetMaxHearts(playerData.currentMaxHP);
        uiController.UpdateHearts(currentHealth, playerData.currentMaxHP);
    }

    /// 사망 처리
    private void Die()
    {
        Debug.Log("[PlayerHealth] 플레이어 사망 처리");
        // 죽음 처리 로직 (애니메이션, 게임 오버 등)
    }
}
