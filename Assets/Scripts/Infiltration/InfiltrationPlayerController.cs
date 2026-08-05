using UnityEngine;
using Infiltration;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(InfiltrationSkillExecutor))]
public class InfiltrationPlayerController : MonoBehaviour
{
    [Header("Config/Refs")]
    public InfiltrationConfigSO config;
    public InfiltrationGridManager grid;
    public LayerMask wallMask;
    public LayerMask obstacleMask;
    public LayerMask enemyMask;

    [Header("Loadout")]
    public InfiltrationLoadoutSO loadout;

    [Header("State")]
    public Vector2Int currentGrid;
    public Vector2Int facing = Vector2Int.right;
    public bool isMovingThisFrame { get; private set; } // FOV 확장을 위한 상태

    [Header("Thief Settings")]
    public string normalLayerName = "Player";
    public string stealthLayerName = "Player_Stealth";

    public bool isStealthMode = false;
    private bool isFearMode;
    private float moveTimer;
    private float dashCooldownTimer;
    private float healCooldownTimer;
    private float parryCooldownTimer;
    private bool parryActive;
    private float parryTimer;

    private float aDownTime = -1f;
    private float spaceDownTime = -1f;

    private PlayerHealth health;
    private InfiltrationSkillExecutor executor;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        executor = GetComponent<InfiltrationSkillExecutor>();
    }

    void Start()
    {
        if (grid == null) grid = InfiltrationGridManager.Instance;
        currentGrid = (grid != null) ? grid.WorldToGrid(transform.position) : Vector2Int.zero;

        if (loadout == null && InfiltrationLoadoutRuntime.Instance != null)
            loadout = InfiltrationLoadoutRuntime.Instance.currentLoadout;

        SnapToGrid(currentGrid);

        // 시작 시 초기 시야 확보
        NotifyVisibilityChanged();
    }

    void Update()
    {
        UpdateCooldowns();

        if (isFearMode)
        {
            HandleFearModeInput();
        }
        else
        {
            HandleMovementInput();
            HandleAttackerInput();
            HandleSupporterInput();
            HandleHealerInput();
            HandleThiefInput();
        }
    }

    // [중요] 시야 업데이트를 매니저에 알리는 중앙 집중 함수
    public void NotifyVisibilityChanged()
    {
        if (InfiltrationVisibilityManager.Instance != null)
        {
            // 이동 중일 때만 facing 방향으로 시야 확장, 아닐 땐 zero 전달 (3x3)
            Vector2Int fovDir = isMovingThisFrame ? facing : Vector2Int.zero;
            InfiltrationVisibilityManager.Instance.UpdateAllVisibility(currentGrid, fovDir);
        }
    }

    void UpdateCooldowns()
    {
        float dt = Time.deltaTime;
        if (moveTimer > 0f) moveTimer -= dt;
        else isMovingThisFrame = false; // 이동 쿨다운이 끝나면 정지 상태로 간주

        if (dashCooldownTimer > 0f) dashCooldownTimer -= dt;
        if (healCooldownTimer > 0f) healCooldownTimer -= dt;
        if (parryCooldownTimer > 0f) parryCooldownTimer -= dt;

        if (parryActive)
        {
            parryTimer -= dt;
            if (parryTimer <= 0f) parryActive = false;
        }
    }

    void HandleFearModeInput()

    {

        //isFearMode, FearMode 시 로직 구현 필요

    }

    // =============== Movement (무한 그리드 적용) ===============
    void HandleMovementInput()
    {
        if (moveTimer > 0f) return;

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;

        if (dir == Vector2Int.zero) return;

        facing = dir;
        Vector2Int next = currentGrid + dir;

        // [수정] IsWithinBounds 검사 제거 (무한 그리드 허용)
        if (IsTileSolid(next)) return;

        currentGrid = next;
        isMovingThisFrame = true; // 시야 확장 트리거
        SnapToGrid(currentGrid);

        moveTimer = config != null ? config.baseMoveCooldown : 0.12f;

        // 이동 시 시야 갱신
        NotifyVisibilityChanged();
    }

    public void SnapToGrid(Vector2Int gridPos)
    {
        transform.position = grid.GridToWorld(gridPos);
        if (facing.x != 0)
        {
            GetComponent<SpriteRenderer>().flipX = facing.x < 0;
        }
    }

    public bool IsTileSolid(Vector2Int gridPos, bool treatObstacleAsSolid = true)
    {
        Vector3 center = grid.GridToWorld(gridPos);
        Vector2 size = Vector2.one * ((grid.config != null ? grid.config.tileSize : 1f) * 0.8f);

        if (Physics2D.OverlapBox(center, size, 0f, wallMask)) return true;
        if (treatObstacleAsSolid && Physics2D.OverlapBox(center, size, 0f, obstacleMask)) return true;
        return false;
    }

    public bool IsObstacleOnTile(Vector2Int gridPos)
    {
        Vector3 center = grid.GridToWorld(gridPos);
        float ts = (grid.config != null) ? grid.config.tileSize : 1f;
        return Physics2D.OverlapBox(center, Vector2.one * (ts * 0.8f), 0f, obstacleMask);
    }

    public IDamageable GetEnemyAt(Vector2Int gridPos)
    {
        Vector3 center = grid.GridToWorld(gridPos);
        float ts = (grid.config != null) ? grid.config.tileSize : 1f;
        Collider2D col = Physics2D.OverlapBox(center, Vector2.one * (ts * 0.8f), 0f, enemyMask);
        if (col == null) return null;
        return col.GetComponent<IDamageable>();
    }

    // =============== Attacker (A: Tap/Hold) ===============
    void HandleAttackerInput()
    {
        // Press
        if (Input.GetKeyDown(KeyCode.A))
            aDownTime = Time.time;

        // Release
        if (Input.GetKeyUp(KeyCode.A))
        {
            float hold = aDownTime >= 0f ? Time.time - aDownTime : 0f;
            aDownTime = -1f;

            float threshold = (config != null) ? config.longPressASeconds : 0.35f;
            bool isHold = hold >= threshold;

            if (loadout != null)
            {
                var skill = isHold ? loadout.A_Hold : loadout.A_Tap;
                if (executor.TryExecute(skill)) return;
            }

            // 폴백(기존)
            if (isHold)
            {
                // AreaSmash(앞 3x3) 간단 폴백
                int depth = 3;
                for (int step = 1; step <= depth; step++)
                {
                    int[] dx = { -1, 0, 1 };
                    foreach (int d in dx)
                    {
                        Vector2Int p = currentGrid + facing * step + (facing.x != 0 ? Vector2Int.up * d : Vector2Int.right * d);
                        if (IsTileSolid(p, true)) continue;
                        var dmg = GetEnemyAt(p);
                        if (dmg != null) dmg.TakeDamage((config != null) ? config.attackerBasicDamage : 0.5f);
                    }
                }
            }
            else
            {
                DoBasicAttack3Tiles();
            }
        }
    }

    void DoBasicAttack3Tiles()
    {
        int maxRange = 3;
        for (int i = 1; i <= maxRange; i++)
        {
            Vector2Int tile = currentGrid + facing * i;

            if (IsTileSolid(tile, treatObstacleAsSolid: true))
                break;

            var hit = GetEnemyAt(tile);
            if (hit != null)
            {
                float dmg = (config != null) ? config.attackerBasicDamage : 0.5f;
                hit.TakeDamage(dmg);
                break;
            }
        }
    }

    // =============== Supporter (이동기 시야 반영) ===============
    void HandleSupporterInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (loadout != null && executor.TryExecute(loadout.Shift))
            {
                NotifyVisibilityChanged(); // 스킬 실행 후 시야 갱신
                return;
            }

            if (dashCooldownTimer > 0f) return;
            DoDash();
            dashCooldownTimer = (config != null) ? config.dashCooldown : 0.6f;
            NotifyVisibilityChanged(); // 대시 후 시야 갱신
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (loadout != null) executor.TryExecute(loadout.S_Tap);
            // 접속/버프는 추후 확장
        }
    }

    void DoDash()
    {
        int maxSteps = (config != null) ? config.dashTiles : 2;
        Vector2Int lastValid = currentGrid;

        for (int step = 1; step <= maxSteps; step++)
        {
            Vector2Int tile = currentGrid + facing * step;
            if (IsTileSolid(tile, treatObstacleAsSolid: false)) break;
            if (IsObstacleOnTile(tile)) break;
            lastValid = tile;
        }

        if (lastValid != currentGrid)
        {
            currentGrid = lastValid;
            isMovingThisFrame = true;
            SnapToGrid(currentGrid);
        }
    }

    // =============== Healer (Space: Tap=Parry, Hold=Heal) ===============
    void HandleHealerInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            spaceDownTime = Time.time;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (spaceDownTime < 0f) return;

            float hold = Time.time - spaceDownTime;
            float threshold = (config != null) ? config.longPressSpaceSeconds : 0.35f;
            bool isHold = hold >= threshold;

            if (loadout != null)
            {
                var skill = isHold ? loadout.Space_Hold : loadout.Space_Tap;
                if (executor.TryExecute(skill)) { spaceDownTime = -1f; return; }
            }

            if (isHold) TryHeal();
            else TryParry();

            spaceDownTime = -1f;
        }
    }

    void TryHeal()
    {
        if (healCooldownTimer > 0f) return;
        float healAmount = (config != null) ? config.healAmountHearts : 1f;
        health.Heal(healAmount);
        healCooldownTimer = (config != null) ? config.healCooldown : 10f;
        Debug.Log($"[Healer] 힐 사용(+{healAmount}). 쿨다운 시작.");
    }

    void TryParry()
    {
        if (parryCooldownTimer > 0f) return;
        ActivateParry((config != null) ? config.parryWindowSeconds : 0.18f);
        parryCooldownTimer = (config != null) ? config.parryCooldown : 10f;
        Debug.Log("[Healer] 패링 윈도우 ON");
    }

    // =============== Thief (C: Stealth Hold, D: Steal Tap – 추후 구현) ===============
    void HandleThiefInput()
    {
        // C키를 누르고 있는 동안 은신 유지
        if (Input.GetKey(KeyCode.C))
        {
            if (!isStealthMode) EnterStealth();

            // 은신 중에도 스킬 실행 (예: 이동속도 증가 등)
            if (loadout != null) executor.TryExecute(loadout.C_Hold);
        }
        // C키를 떼면 은신 해제
        else if (isStealthMode)
        {
            ExitStealth();
        }

        // D키: 훔치기 (짧게 누르기)
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (loadout != null) executor.TryExecute(loadout.D_Tap);
            // 훔치기 로직(인접 적 체크 등)은 executor 내에서 처리 권장
        }
    }

    void EnterStealth()
    {
        isStealthMode = true;
        gameObject.layer = LayerMask.NameToLayer(stealthLayerName);

        var sr = GetComponent<SpriteRenderer>();
        Color c = sr.color; c.a = 0.5f; sr.color = c;

        if (config != null)
            moveTimer = config.baseMoveCooldown * config.stealthMoveCooldownMultiplier;

        // 은신 진입 시에도 주변 시야 재확인 가능 (선택 사항)
        NotifyVisibilityChanged();
    }

    void ExitStealth()
    {
        isStealthMode = false;
        gameObject.layer = LayerMask.NameToLayer(normalLayerName);

        var sr = GetComponent<SpriteRenderer>();
        Color c = sr.color; c.a = 1.0f; sr.color = c;

        NotifyVisibilityChanged();
    }

    // 외부/실행기 접근용
    public void ActivateParry(float window)
    {
        parryActive = true;
        parryTimer = Mathf.Max(0.05f, window);
    }

    public void OnIncomingAttack(float damage, GameObject attacker)
    {
        if (parryActive && attacker != null)
        {
            var d = attacker.GetComponent<IDamageable>();
            if (d != null)
            {
                d.TakeDamage(damage);
                Debug.Log("[Healer] 패링 성공! 반사 피해 적용");
            }
            parryActive = false;
            return;
        }
        health.TakeDamage(damage);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (grid == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 1; i <= 3; i++)
        {
            Vector2Int t = currentGrid + facing * i;
            Gizmos.DrawWireCube(grid.GridToWorld(t), Vector3.one * 0.8f);
        }
    }
#endif
}