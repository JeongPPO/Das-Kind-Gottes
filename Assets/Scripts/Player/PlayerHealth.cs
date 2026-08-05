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
    private bool isInvincible = false;

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
        if (isInvincible)
        {
            Debug.Log("[PlayerHealth] 무적 상태이므로 피해를 입지 않습니다.");
            return;
        }

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

    public void SetInvincible(float duration)
    {
        StartCoroutine(InvincibleRoutine(duration));
    }

    private IEnumerator InvincibleRoutine(float duration)
    {
        isInvincible = true;
        float elapsed = 0f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;

        while (elapsed < duration)
        {
            // 깜빡임 연출 (알파값 조절)
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        sr.color = originalColor;
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] 플레이어 사망 처리");

        // LifeManager가 붙어있는지 확인하고 사망 로직 실행
        var lifeManager = GetComponent<InfiltrationPlayerLifeManager>();
        if (lifeManager != null)
        {
            lifeManager.HandleDeath();
        }
        else
        {
            // 잠입 씬이 아닌 일반 씬에서의 기본 사망 처리
            // 예: 씬 재시작 등
        }
    }
}
