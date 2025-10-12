using UnityEngine;
using UnityEngine.Tilemaps;

public class Player2Controller : MonoBehaviour
{
    [Header("공격 설정")]
    public float baseAttackPower = 10f;
    public float attackRange = 1f;

    [Header("대시 설정")]
    public int baseDashDistance = 2;
    public int dashGrowthBonus = 0;
    public float dashBuffDuration = 2f;
    public float dashCooldown = 1f;

    [Header("레퍼런스")]
    public PlayerData playerData;
    public HeartUIController uiController;
    public PlayerHealth playerHealth;

    [Header("타일맵 및 하이라이트 타일")]
    public Tilemap tilemap; // Inspector에서 할당
    public TileBase blueHighlightTile; // Inspector에서 할당

    // 내부 상태
    private bool dashBuffActive = false;
    private float dashBuffTimer = 0f;
    private int dashAttackCount = 0;

    private Vector2Int previousGridPosition;
    private Vector2Int lastDashDirection = Vector2Int.zero;

    private bool canDash = true;
    private float dashCooldownTimer = 0f;

    private bool canBackwardDash = false; // 대시 직후 한 번만 백워드 가능
    private float backwardDashWindowTimer = 0f;
    private const float backwardDashWindowDuration = 3f;

    private BaseMovement baseMovement;

    private struct HighlightTileInfo
    {
        public Vector3Int cell;
        public TileBase originalTile;
    }
    private HighlightTileInfo? highlightedTileInfo = null;

    // 추가: 대시 도착 위치 저장용 변수
    private Vector2Int lastDashTargetPosition;

    void Start()
    {
        baseMovement = GetComponentInParent<BaseMovement>();
        if (baseMovement == null)
        {
            Debug.LogError("BaseMovement를 찾을 수 없습니다! Player2는 부모 오브젝트에 BaseMovement가 필요합니다.");
        }
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        HandleAttackInput();
        UpdateDashBuff();
        UpdateDashCooldown();
        HandleBackwardDashWindow();
        HandleDashInput();
        UpdateBackwardDashTileHighlight();
    }

    // ========================= 공격 =========================
    void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float finalDamage = baseAttackPower;

                if (dashBuffActive)
                {
                    dashAttackCount++;
                    if (dashAttackCount == 1)
                        finalDamage *= 2f;      // 1타
                    else if (dashAttackCount <= 3)
                        finalDamage *= 1.5f;    // 2~3타
                }

                damageable.TakeDamage(finalDamage);
                FearGaugeManager.Instance.AddDamage(finalDamage);

                Debug.Log($"플레이어2 공격! 최종 피해: {finalDamage}");
                break;
            }
        }
    }


    void UpdateDashBuff()
    {
        if (dashBuffActive)
        {
            dashBuffTimer -= Time.deltaTime;
            if (dashBuffTimer <= 0f)
            {
                dashBuffActive = false;
                dashAttackCount = 0;
                Debug.Log("대시 버프 종료");
            }
        }
    }

    void UpdateDashCooldown()
    {
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
                Debug.Log("대시 쿨다운 완료");
            }
        }
    }

    // ========================= 대시 =========================
    void HandleBackwardDashWindow()
    {
        if (canBackwardDash)
        {
            backwardDashWindowTimer -= Time.deltaTime;
            if (backwardDashWindowTimer <= 0f)
            {
                canBackwardDash = false;
                Debug.Log("백워드 대시 시간 종료");
            }
        }
    }

    void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (canDash)
            {
                Vector2Int dashDir = GetDashDirection();
                if (dashDir != Vector2Int.zero)
                {
                    Dash(dashDir);
                }
            }
        }

        // 🔹 백워드 대시: 대시 직후 Ctrl 키로 한 번만 발동
        if (canBackwardDash && Input.GetKeyDown(KeyCode.LeftControl))
        {
            BackwardDash();
        }
    }

    void UpdateBackwardDashTileHighlight()
    {
        if (canBackwardDash)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(lastDashTargetPosition);
            Vector3Int tilePos = tilemap.WorldToCell(worldPos);

            // 기존 하이라이트가 다른 위치에 있으면 복원
            if (highlightedTileInfo != null && highlightedTileInfo.Value.cell != tilePos)
            {
                tilemap.SetTile(highlightedTileInfo.Value.cell, highlightedTileInfo.Value.originalTile);
                highlightedTileInfo = null;
            }

            // 새 위치에 하이라이트 타일 설치
            if (highlightedTileInfo == null)
            {
                TileBase originalTile = tilemap.GetTile(tilePos);
                tilemap.SetTile(tilePos, blueHighlightTile);
                highlightedTileInfo = new HighlightTileInfo { cell = tilePos, originalTile = originalTile };
            }
        }
        else
        {
            // 하이라이트 타일 제거 및 원래 타일 복원
            if (highlightedTileInfo != null)
            {
                tilemap.SetTile(highlightedTileInfo.Value.cell, highlightedTileInfo.Value.originalTile);
                highlightedTileInfo = null;
            }
        }
    }

    void BackwardDash()
    {
        if (!canBackwardDash) return;

        baseMovement.currentGridPosition = lastDashTargetPosition;
        baseMovement.MoveToPosition(lastDashTargetPosition);
        canBackwardDash = false;

        // 하이라이트 제거
        UpdateBackwardDashTileHighlight();

        Debug.Log("백워드 대시 발동!");
    }


    Vector2Int GetDashDirection()
    {
        // 하나의 키만 우선 적용: 상 > 하 > 왼 > 오
        if (Input.GetKey(KeyCode.UpArrow)) return Vector2Int.up;
        if (Input.GetKey(KeyCode.DownArrow)) return Vector2Int.down;
        if (Input.GetKey(KeyCode.LeftArrow)) return Vector2Int.left;
        if (Input.GetKey(KeyCode.RightArrow)) return Vector2Int.right;
        return Vector2Int.zero;
    }

    void Dash(Vector2Int direction)
    {
        previousGridPosition = baseMovement.currentGridPosition;
        lastDashDirection = direction;

        int dashDistance = baseDashDistance + dashGrowthBonus;
        Vector2Int targetPos = previousGridPosition + direction * dashDistance;
        targetPos = ClampToBounds(targetPos);

        baseMovement.currentGridPosition = targetPos;
        baseMovement.MoveToPosition(targetPos);

        // 대시 도착 위치 저장
        lastDashTargetPosition = targetPos;

        dashBuffActive = true;
        dashBuffTimer = dashBuffDuration;
        dashAttackCount = 0;

        canDash = false;
        dashCooldownTimer = dashCooldown;

        canBackwardDash = true;
        backwardDashWindowTimer = backwardDashWindowDuration;

        Debug.Log($"플레이어2 대시 완료! 최종 위치: {targetPos}");
    }

    Vector2Int ClampToBounds(Vector2Int pos)
    {
        int minX = -6, maxX = 5, minY = -2, maxY = 4;
        return new Vector2Int(
            Mathf.Clamp(pos.x, minX, maxX),
            Mathf.Clamp(pos.y, minY, maxY)
        );
    }
}
