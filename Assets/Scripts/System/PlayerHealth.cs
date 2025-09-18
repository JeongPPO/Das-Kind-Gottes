using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public PlayerData playerData;
    public HeartUIController uiController;
    private PlayerStatus status;

    public float currentHealth;
    public bool isLethalOnHit = false;  // 즉사 모드 여부
    public bool canHeal = true; // 회복 가능 여부

    IEnumerator Start()
    {
        yield return null; // 한 프레임 대기
        Debug.Log($"플레이어 최대 체력: {playerData.currentMaxHP}");
        currentHealth = playerData.currentMaxHP;
        uiController.SetMaxHearts(playerData.currentMaxHP);
        uiController.UpdateHearts(currentHealth, playerData.currentMaxHP);
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Debug.Log($"체력 감소: {amount}, 현재 체력: {currentHealth}");
        uiController.UpdateHearts(currentHealth, playerData.currentMaxHP);
        if (isLethalOnHit)
        {
            status.currentHP = 0;
            Debug.Log("즉사 디버프 적용됨! 플레이어 사망");
            Die();
            return;
        }
        status.currentHP -= damage;
        if (status.currentHP <= 0)
        {
            status.currentHP = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!canHeal)
        {
            Debug.Log("회복 불가 상태");
            return;
        }
        currentHealth = Mathf.Min(playerData.currentMaxHP, currentHealth + amount);
        uiController.UpdateHearts(currentHealth, playerData.currentMaxHP);
    }

    public void IncreaseMaxHealth(float amount)
    {
        playerData.IncreaseMaxHP(amount);
        uiController.SetMaxHearts(playerData.currentMaxHP);
        uiController.UpdateHearts(currentHealth, playerData.currentMaxHP);
    }
    private void Die()
    {
        Debug.Log("플레이어 사망 처리");
        // 죽음 처리 로직 (애니메이션, 게임 오버 등)
    }
}
