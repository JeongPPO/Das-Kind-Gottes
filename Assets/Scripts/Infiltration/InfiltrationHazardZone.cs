using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InfiltrationHazardZone : MonoBehaviour
{
    public InfiltrationConfigSO config;
    public LayerMask affectedMask; // 플레이어만 맞게 설정 권장

    private Dictionary<Collider2D, float> lastTickTime = new Dictionary<Collider2D, float>();

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAffected(other)) return;

        float dmg = (config != null) ? config.hazardDamageOnEnter : 0.25f;
        ApplyDamage(other, dmg);

        lastTickTime[other] = Time.time;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!IsAffected(other)) return;

        float last;
        if (!lastTickTime.TryGetValue(other, out last))
            last = 0f;

        float interval = (config != null) ? config.hazardTickInterval : 1f;
        if (Time.time - last >= interval)
        {
            float dot = (config != null) ? config.hazardDamagePerTick : 0.25f;
            ApplyDamage(other, dot);
            lastTickTime[other] = Time.time;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (lastTickTime.ContainsKey(other))
            lastTickTime.Remove(other);
    }

    bool IsAffected(Collider2D col)
    {
        return ((affectedMask.value & (1 << col.gameObject.layer)) != 0);
    }

    void ApplyDamage(Collider2D col, float amount)
    {
        var ph = col.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(amount);
            // 깜빡임 등 무적 프레임은 PlayerHealth 쪽 확장으로 처리 가능
        }
    }
}