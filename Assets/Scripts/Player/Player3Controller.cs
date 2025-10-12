using UnityEngine;
using UnityEngine.Tilemaps;

public class Player3Controller : MonoBehaviour
{
    [Header("공격 관련")]
    public float attackRange = 1f;
    public float attackPower = 6f;
    public float critMultiplier = 2f;

    [Header("가드 관련")]
    public float guardStrength = 50f;   // 막을 수 있는 적의 공격 데미지 크기
    private bool isGuarding = false;

    [Header("패링 관련")]
    public float parryCooldown = 3f;
    public float parryWindow = 0.3f;    // 입력 직후 유효 시간
    public float parryDamageMultiplier = 2f; // 반사 배율
    private float lastParryTime = -999f;
    private bool parryActive = false;

    [Header("힐 존 관련")]
    public float healZoneCooldown = 5f;
    public float healZoneDuration = 3f;

    [Header("힐 존 타일맵 및 타일")]
    public Tilemap healZoneTilemap; // Inspector에서 할당
    public TileBase healZoneTile;   // Inspector에서 할당

    [Header("레퍼런스")]
    public PlayerData playerData;
    public PlayerHealth playerHealth;
    public BaseMovement baseMovement; // Inspector에서 할당 또는 GetComponentInParent로 할당

    private float lastHealZoneTime = -999f;
    private struct HealZoneCellInfo
    {
        public Vector3Int cell;
        public TileBase originalTile;
    }
    private HealZoneCellInfo[] lastHealZoneCells = null;

    void Update()
    {
        if (!gameObject.activeSelf) return;

        HandleInput();
    }

    void HandleInput()
    {
        // 공격
        if (Input.GetKeyDown(KeyCode.Space))
            TryAttack();

        // 가드
        if (Input.GetKeyDown(KeyCode.LeftShift))
            StartGuard();
        if (Input.GetKeyUp(KeyCode.LeftShift))
            StopGuard();

        // 패링 (가드 중에만 가능)
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGuarding)
            TryParry();

        // 힐 존 설치
        if (Input.GetKeyDown(KeyCode.Q))
            TryPlaceHealZone();
    }

    void TryAttack()
    {
        // TODO: 공격 구현
    }

    void StartGuard()
    {
        isGuarding = true;
        // 이동 불가 처리 → BaseMovement의 이동 입력 무시 플래그 필요
        BaseMovement bm = GetComponentInParent<BaseMovement>();
        if (bm != null) bm.enabled = false;
    }

    void StopGuard()
    {
        isGuarding = false;
        BaseMovement bm = GetComponentInParent<BaseMovement>();
        if (bm != null) bm.enabled = true;
    }

    void TryParry()
    {
        if (Time.time - lastParryTime < parryCooldown) return;

        parryActive = true;
        lastParryTime = Time.time;

        // 일정 시간 후 패링 윈도우 종료
        Invoke(nameof(EndParryWindow), parryWindow);
    }

    void EndParryWindow()
    {
        parryActive = false;
    }

    public void OnIncomingAttack(float damage, GameObject attacker)
    {
        if (parryActive)
        {
            // 패링 성공
            float reflectedDamage = damage * parryDamageMultiplier;
            attacker.GetComponent<Boss>().TakeDamage(reflectedDamage);

            // 강화 효과 트리거 가능 (예: 다음 공격 크리 확정)
            Debug.Log("패링 성공! 반사 피해: " + reflectedDamage);
            parryActive = false;
        }
        else if (isGuarding)
        {
            // 가드 중일 경우 → guardStrength 소모
            float blocked = Mathf.Min(damage, guardStrength);
            guardStrength -= blocked;
            float leftover = damage - blocked;
            if (leftover > 0) playerHealth.TakeDamage(leftover);
        }
        else
        {
            // 그냥 맞음
            playerHealth.TakeDamage(damage);
        }
    }

    void TryPlaceHealZone()
    {
        if (Time.time - lastHealZoneTime < healZoneCooldown) return;

        Vector3Int centerCell = healZoneTilemap.WorldToCell(transform.position);

        var cellList = new System.Collections.Generic.List<HealZoneCellInfo>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector3Int cell = new Vector3Int(centerCell.x + dx, centerCell.y + dy, centerCell.z);

                // x: -6 ~ 6, y: 3 ~ -4 범위 체크
                if (cell.x >= -6 && cell.x <= 5 && cell.y <= 3 && cell.y >= -4)
                {
                    TileBase originalTile = healZoneTilemap.GetTile(cell);
                    healZoneTilemap.SetTile(cell, healZoneTile);
                    cellList.Add(new HealZoneCellInfo { cell = cell, originalTile = originalTile });
                }
            }
        }

        lastHealZoneCells = cellList.ToArray();
        lastHealZoneTime = Time.time;
        Invoke(nameof(RemoveHealZoneTiles), healZoneDuration);
    }

    void RemoveHealZoneTiles()
    {
        if (lastHealZoneCells == null) return;
        foreach (var info in lastHealZoneCells)
        {
            healZoneTilemap.SetTile(info.cell, info.originalTile); // 원래 타일로 복원
        }
        lastHealZoneCells = null;
    }
}
