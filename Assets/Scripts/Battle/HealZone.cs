using UnityEngine;
using System.Collections.Generic;

public class HealZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public float duration = 6f;          // 몇 초 동안 유지되는가
    public float radius = 2.5f;          // 범위
    public float healPerSecond = 2f;     // 초당 회복량
    public float damageReduction = 0.1f; // 10% 감소

    private float spawnTime;

    // 이 안에 들어온 플레이어들
    private List<PlayerHealth> playersInZone = new List<PlayerHealth>();

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // 지속 시간 끝나면 삭제
        if (Time.time - spawnTime > duration)
        {
            Destroy(gameObject);
            return;
        }

        // 1초마다 힐 적용
        foreach (var player in playersInZone)
        {
            if (player != null)
            {
                player.Heal(healPerSecond * Time.deltaTime); 
                player.SetDamageReduction(damageReduction); 
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null && !playersInZone.Contains(ph))
        {
            playersInZone.Add(ph);
            ph.SetDamageReduction(damageReduction);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null && playersInZone.Contains(ph))
        {
            playersInZone.Remove(ph);
            ph.SetDamageReduction(0f); // 존을 벗어나면 원래대로
        }
    }

    void OnDestroy()
    {
        // 존이 사라질 때 버프 해제
        foreach (var player in playersInZone)
        {
            if (player != null)
                player.SetDamageReduction(0f);
        }
    }
}
