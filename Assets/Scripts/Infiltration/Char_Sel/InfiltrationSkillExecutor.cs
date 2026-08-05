using System.Collections.Generic;
using UnityEngine;
using Infiltration;

[RequireComponent(typeof(InfiltrationPlayerController))]
public class InfiltrationSkillExecutor : MonoBehaviour
{
    private InfiltrationPlayerController pc;
    private InfiltrationGridManager grid;
    private PlayerHealth health;

    private readonly Dictionary<InfiltrationSkillSO, float> nextReadyTime = new();

    void Awake()
    {
        pc = GetComponent<InfiltrationPlayerController>();
        grid = InfiltrationGridManager.Instance;
        health = GetComponent<PlayerHealth>();
    }

    public bool TryExecute(InfiltrationSkillSO skill)
    {
        if (skill == null) return false;

        float tNow = Time.time;
        if (nextReadyTime.TryGetValue(skill, out var tReady) && tNow < tReady)
            return false;

        bool ok = ExecuteInternal(skill);

        if (ok)
        {
            // [확장성 핵심] 스킬 성공 시 공통 이펙트 생성 로직
            SpawnEffect(skill);

            if (skill.cooldown > 0f)
                nextReadyTime[skill] = tNow + skill.cooldown;
        }

        return ok;
    }

    bool ExecuteInternal(InfiltrationSkillSO s)
    {
        switch (s.type)
        {
            case SkillType.LineAttack:
                return Exec_LineAttack(s);
            case SkillType.AreaSmash:
                return Exec_AreaSmash(s);
            case SkillType.Dash:
                return Exec_Dash(s);
            case SkillType.Heal:
                return Exec_Heal(s);
            case SkillType.Parry:
                return Exec_Parry(s);
            // Thief/Support 확장은 추후 추가(스텔스/훔치기/버프)
            default:
                Debug.LogWarning($"[SkillExecutor] 미지원 스킬: {s.type}");
                return false;
        }
    }

    bool Exec_LineAttack(InfiltrationSkillSO s)
    {
        int maxRange = Mathf.Max(1, s.rangeTiles);
        for (int i = 1; i <= maxRange; i++)
        {
            var tile = pc.currentGrid + pc.facing * i;
            if (pc.IsTileSolid(tile, treatObstacleAsSolid: true))
                break;

            var hit = pc.GetEnemyAt(tile);
            if (hit != null)
            {
                hit.TakeDamage(Mathf.Max(0f, s.damageHearts));
                return true;
            }
        }
        return true; // 사용은 했다고 가정(타격 실패 가능)
    }

    bool Exec_AreaSmash(InfiltrationSkillSO s)
    {
        // 앞 3줄 × 3열의 가운데 정렬(요구사항 기준) - 간단 구현
        int depth = Mathf.Max(1, s.rangeTiles); // 기본 3줄 가정
        var origin = pc.currentGrid;

        int[] dx = { -1, 0, 1 };
        for (int step = 1; step <= depth; step++)
        {
            for (int i = 0; i < dx.Length; i++)
            {
                Vector2Int p = origin + pc.facing * step;
                // 좌우 확장
                if (pc.facing.x != 0) p += Vector2Int.up * dx[i];
                else p += Vector2Int.right * dx[i];

                if (pc.IsTileSolid(p, true)) continue;

                var d = pc.GetEnemyAt(p);
                if (d != null) d.TakeDamage(Mathf.Max(0f, s.damageHearts));
            }
        }
        return true;
    }

    bool Exec_Dash(InfiltrationSkillSO s)
    {
        int maxSteps = Mathf.Max(1, s.rangeTiles);
        Vector2Int lastValid = pc.currentGrid;

        for (int step = 1; step <= maxSteps; step++)
        {
            var tile = pc.currentGrid + pc.facing * step;

            if (pc.IsTileSolid(tile, treatObstacleAsSolid: false)) // 벽만 불가
                break;

            if (pc.IsObstacleOnTile(tile)) // 장애물 위 착지 불가
                break;

            lastValid = tile;
        }

        if (lastValid != pc.currentGrid)
        {
            pc.currentGrid = lastValid;
            pc.SnapToGrid(pc.currentGrid);
        }
        return true;
    }

    bool Exec_Heal(InfiltrationSkillSO s)
    {
        health.Heal(Mathf.Max(0f, s.healHearts));
        return true;
    }

    bool Exec_Parry(InfiltrationSkillSO s)
    {
        pc.ActivateParry(Mathf.Max(0.05f, s.parryWindow));
        return true;
    }

    bool Exec_Stealth(InfiltrationSkillSO s)
    {
        // s.stunSeconds를 은신 지속시간으로 활용하거나 별도 변수 활용
        // PlayerController의 은신 로직 호출 (이전 답변에서 구현한 EnterStealth 등)
        // pc.EnterStealth(s.duration); 
        // 사실상 EnterStealth 로직은 PlayerController에 이미 있지만,
        // Executor를 거침으로써 스킬 데이터(SO)에 정의된 수치를 반영할 수 있습니다.

        // 예: 은신 중 이동 속도 감소 등을 Config에서 가져오거나 SO에서 가져옴
        float stealthMovePenalty = 1.5f; // 평소보다 1.5배 느리게 (이동 쿨다운 증가)

        // 이 메서드는 C키를 '누르고 있는 동안' 매 프레임 호출될 수 있으므로
        // 이미 은신 중이라면 추가 로직을 타지 않게 설계합니다.
        return true;
    }

    bool Exec_Steal(InfiltrationSkillSO s)
    {
        // 1. 은신 상태 체크
        if (s.requireStealth && !pc.isStealthMode)
        {
            Debug.Log("은신 상태에서만 '기습'을 사용할 수 있습니다!");
            return false;
        }

        // 2. 인접한 적 확인
        Vector2Int targetTile = pc.currentGrid + pc.facing;
        var target = pc.GetEnemyAt(targetTile);

        if (target != null)
        {
            // 3. 아이템 획득 로직 (먼저 실행)
            string stolenItem = GetRandomItemFromEnemy(target);
            Debug.Log($"<color=yellow>[Steal Success]</color> {stolenItem}을(를) 획득했습니다!");

            // 4. 공격 로직 (처치)
            target.TakeDamage(s.damageHearts);

            // 5. 공포 게이지 대폭 상승
            // FearGaugeManager.Instance.AddGauge(25f);

            // 연출: 암살 성공 이펙트 생성
            if (s.effectPrefab != null) Instantiate(s.effectPrefab, pc.grid.GridToWorld(targetTile), Quaternion.identity);

            return true;
        }

        return false;
    }

    private void SpawnEffect(InfiltrationSkillSO s)
    {
        // 1. 프리팹이 할당되어 있다면 그것을 생성
        if (s.effectPrefab != null)
        {
            GameObject fx = Instantiate(s.effectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, s.effectDestroyTime);
        }
        // 2. [테스트용] 프리팹이 없다면 임시 프리미티브 생성 (확인용)
        else
        {
            // 기습(Steal)이나 공격 같은 경우 적의 위치에 생성하면 좋으므로 위치 계산
            Vector3 spawnPos = transform.position + (Vector3)((Vector2)pc.facing * 0.5f);

            GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCube.transform.position = spawnPos;
            debugCube.transform.localScale = Vector3.one * 0.3f;

            // 박스 컬라이더가 생성되어 물리 충돌이 날 수 있으므로 제거
            Destroy(debugCube.GetComponent<BoxCollider>());
            // 시각적으로 튀게 색상 변경 (도적이면 파랑, 어태커면 빨강 등)
            debugCube.GetComponent<Renderer>().material.color = GetRoleColor(s.role);

            Destroy(debugCube, 0.5f);
        }
    }

    private string GetRandomItemFromEnemy(IDamageable enemy)
    {
        // 나중에 Enemy 클래스에서 아이템 테이블을 가져오도록 확장 가능
        return "공포 카드키 조각";
    }

    private Color GetRoleColor(RoleType role)
    {
        return role switch
        {
            RoleType.Attacker => Color.red,
            RoleType.Supporter => Color.blue,
            RoleType.Thief => Color.white,
            RoleType.Healer => Color.green,
            _ => Color.yellow
        };
    }
}