using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class JamipController : MonoBehaviour
{
    [Header("References")]
    public JamipMapManager mapManager;
    public Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    [Header("Grid Movement")]
    public Vector2Int currentGridPosition = Vector2Int.zero;
    [SerializeField] private float baseMoveCooldown = 0.05f;
    private float moveCooldown;
    private float moveTimer = 0f;

    [Header("Tile Layout (Rectangular)")]
    [Range(1, 10)] public int lanesCount = 3;
    [Range(0.2f, 0.9f)] public float laneFraction = 0.6f;
    public int visibleColumnsHorizontal = 9;
    public int visibleRowsVertical = 7;

    private float tileWidth = 1f;
    private float tileHeight = 1f;

    [Header("Stealth")]
    public bool isStealthed = false;
    public float stealthAlpha = 0.4f;
    public bool canSteal = false;
    public bool canConnect = false;

    [Header("Assault")]
    public float attackRange = 1f;
    public LayerMask enemyLayer = ~0;

    public System.Action OnMissionFailed;

    [Header("Connect")]
    [Tooltip("접속 유지 이동 횟수")]
    public int connectMaxSteps = 8;
    private int connectStepsLeft = 0;
    private JamipEnemyTarget connectedTarget;
    private Sprite originalSprite;

    private Vector2Int lastInputDir = Vector2Int.right;

    [Header("그리드 체크용")]
    public bool IsVertical => IsVerticalScroll();
    public float TileWidth => tileWidth;
    public float TileHeight => tileHeight;
    public int LanesCount => lanesCount;

    private readonly Collider2D[] overlapBuffer = new Collider2D[16];
    private ContactFilter2D enemyFilter;

    public void GetVisibleProgressRange(out int minProg, out int maxProg) => GetVisibleGridWindow(out minProg, out maxProg);

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ConfigureEnemyFilter();
    }

    void Start()
    {
        moveCooldown = baseMoveCooldown;

        if (mapManager == null)
            mapManager = FindFirstObjectByType<JamipMapManager>();

        if (mapManager == null || mapManager.currentSegment == null)
        {
            Debug.LogError("❌ MapManager 또는 세그먼트가 초기화되지 않았습니다!");
            enabled = false;
            return;
        }

        ApplySegmentSettings(mapManager.currentSegment);
    }

    void Update()
    {
        if (!HasSegment) return;

        HandleMovement();
        HandleAbilities();
        HandleAssault();
    }

    void LateUpdate()
    {
        if (!HasSegment) return;
        CheckFailCondition();
    }

    bool HasSegment => mapManager != null && mapManager.currentSegment != null;

    // ========================= 이동 =========================
    void HandleMovement()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer > 0f) return;

        Vector2Int dir = ReadDiscreteInput();
        if (dir == Vector2Int.zero) return;

        lastInputDir = dir;

        GetVisibleGridWindow(out int minProg, out int maxProg);
        bool verticalScroll = IsVerticalScroll();
        Vector2Int nextGrid = currentGridPosition + dir;

        if (verticalScroll)
        {
            if (nextGrid.x < 0 || nextGrid.x >= lanesCount) return;
            if (nextGrid.y < minProg || nextGrid.y > maxProg) return;
        }
        else
        {
            if (nextGrid.y < 0 || nextGrid.y >= lanesCount) return;
            if (nextGrid.x < minProg || nextGrid.x > maxProg) return;
        }

        if (IsGridBlockedByEnemy(nextGrid)) return;

        currentGridPosition = nextGrid;
        MoveToGrid(currentGridPosition);
        moveTimer = moveCooldown;

        if (connectedTarget != null)
        {
            connectStepsLeft = Mathf.Max(0, connectStepsLeft - 1);
            if (connectStepsLeft <= 0) EndConnect();
        }
    }

    Vector2Int ReadDiscreteInput()
    {
        if (Input.GetKey(KeyCode.UpArrow)) return Vector2Int.up;
        if (Input.GetKey(KeyCode.DownArrow)) return Vector2Int.down;
        if (Input.GetKey(KeyCode.LeftArrow)) return Vector2Int.left;
        if (Input.GetKey(KeyCode.RightArrow)) return Vector2Int.right;
        return Vector2Int.zero;
    }

    bool IsGridBlockedByEnemy(Vector2Int grid)
    {
        Vector3 center = GetWorldCenterOfGrid(grid);
        int cnt = Physics2D.OverlapCircle(center, Mathf.Min(tileWidth, tileHeight) * 0.45f, enemyFilter, overlapBuffer);
        for (int i = 0; i < cnt; i++)
        {
            if (overlapBuffer[i] && overlapBuffer[i].GetComponent<JamipEnemyTarget>())
                return true;
        }
        return false;
    }

    Vector3 GetWorldCenterOfGrid(Vector2Int gridPos)
    {
        Vector3 camPos = mainCamera.transform.position;
        bool verticalScroll = IsVerticalScroll();
        if (verticalScroll)
        {
            float laneRegionWidth = lanesCount * tileWidth;
            float laneOriginX = camPos.x - laneRegionWidth * 0.5f + tileWidth * 0.5f;
            float x = laneOriginX + gridPos.x * tileWidth;
            float y = gridPos.y * tileHeight + tileHeight * 0.5f;
            return new Vector3(x, y, 0f);
        }
        else
        {
            float laneRegionHeight = lanesCount * tileHeight;
            float laneOriginY = camPos.y - laneRegionHeight * 0.5f + tileHeight * 0.5f;
            float y = laneOriginY + gridPos.y * tileHeight;
            float x = gridPos.x * tileWidth + tileWidth * 0.5f;
            return new Vector3(x, y, 0f);
        }
    }

    // ========================= 세그먼트 적용 =========================
    public void ApplySegmentSettings(MapSegment segment)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("❌ 메인 카메라를 찾을 수 없습니다.");
            return;
        }

        ComputeTileSizes();

        segment.tileSize = tileHeight;

        var camScroll = mainCamera.GetComponent<CameraAutoScroll>();
        if (camScroll != null)
        {
            camScroll.scrollDir = segment.direction;
            camScroll.scrollSpeed = segment.cameraScrollSpeed;
            camScroll.playerTransform = transform;
            camScroll.failMargin = segment.failMargin;
        }

        currentGridPosition = segment.playerStartGrid;
        MoveToGrid(currentGridPosition);

        Debug.Log($"[ApplySegmentSettings] {segment.name} → tileW={tileWidth:F3}, tileH={tileHeight:F3}");
    }

    void ComputeTileSizes()
    {
        float orthoH = 2f * mainCamera.orthographicSize;
        float orthoW = orthoH * mainCamera.aspect;

        bool verticalScroll = IsVerticalScroll();
        float frac = Mathf.Clamp(laneFraction, 0.2f, 0.9f);
        int lanes = Mathf.Max(1, lanesCount);

        if (verticalScroll)
        {
            float laneRegionW = orthoW * frac;
            tileWidth = laneRegionW / lanes;
            tileHeight = orthoH / Mathf.Max(1, visibleRowsVertical);
        }
        else
        {
            float laneRegionH = orthoH * frac;
            tileHeight = laneRegionH / lanes;
            tileWidth = orthoW / Mathf.Max(1, visibleColumnsHorizontal);
        }
    }

    bool IsVerticalScroll()
    {
        var seg = mapManager.currentSegment;
        var dir = seg.direction;
        return dir == CameraAutoScroll.ScrollDirection.Up || dir == CameraAutoScroll.ScrollDirection.Down;
    }

    void GetVisibleGridWindow(out int minProg, out int maxProg)
    {
        Vector3 camPos = mainCamera.transform.position;
        float orthoH = 2f * mainCamera.orthographicSize;
        float orthoW = orthoH * mainCamera.aspect;

        bool verticalScroll = IsVerticalScroll();

        if (verticalScroll)
        {
            float bottom = camPos.y - orthoH * 0.5f;
            float top = camPos.y + orthoH * 0.5f;
            minProg = Mathf.FloorToInt(bottom / tileHeight);
            maxProg = Mathf.FloorToInt(top / tileHeight);
        }
        else
        {
            float left = camPos.x - orthoW * 0.5f;
            float right = camPos.x + orthoW * 0.5f;
            minProg = Mathf.FloorToInt(left / tileWidth);
            maxProg = Mathf.FloorToInt(right / tileWidth);
        }
    }

    void MoveToGrid(Vector2Int gridPos)
    {
        Vector3 camPos = mainCamera.transform.position;
        bool verticalScroll = IsVerticalScroll();

        float worldX;
        float worldY;

        if (verticalScroll)
        {
            float laneRegionWidth = lanesCount * tileWidth;
            float laneOriginX = camPos.x - laneRegionWidth * 0.5f + tileWidth * 0.5f;
            worldX = laneOriginX + gridPos.x * tileWidth;
            worldY = gridPos.y * tileHeight + tileHeight * 0.5f;
        }
        else
        {
            float laneRegionHeight = lanesCount * tileHeight;
            float laneOriginY = camPos.y - laneRegionHeight * 0.5f + tileHeight * 0.5f;
            worldY = laneOriginY + gridPos.y * tileHeight;
            worldX = gridPos.x * tileWidth + tileWidth * 0.5f;
        }

        transform.position = new Vector3(worldX, worldY, transform.position.z);
    }

    // ========================= 능력 =========================
    void HandleAbilities()
    {
        EnableStealth(Input.GetKey(KeyCode.Q));

        if (Input.GetKeyDown(KeyCode.W) && isStealthed)
            DoOnNearbyEnemy(e => e.HasItem, e => { e.OnStolen(); });

        if (Input.GetKeyDown(KeyCode.E) && isStealthed)
            DoOnNearbyEnemy(e => e.CanConnect, BeginConnect);
    }

    public void EnableStealth(bool active)
    {
        if (isStealthed == active) return;

        isStealthed = active;
        canSteal = active;
        canConnect = active;

        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        var c = spriteRenderer.color;
        c.a = active ? stealthAlpha : 1f;
        spriteRenderer.color = c;

        moveCooldown = active ? baseMoveCooldown * 1.5f : baseMoveCooldown;
    }

    // 인접 칸 대상 선택(방향 가중치 포함)
    void DoOnNearbyEnemy(System.Func<JamipEnemyTarget, bool> predicate, System.Action<JamipEnemyTarget> action)
    {
        int cnt = Physics2D.OverlapCircle(transform.position, tileWidth * 1.1f, enemyFilter, overlapBuffer);
        if (cnt <= 0) return;

        JamipEnemyTarget chosen = null;
        float bestScore = float.NegativeInfinity;
        Vector2 faceDir = new Vector2(lastInputDir.x, lastInputDir.y).normalized;
        if (faceDir == Vector2.zero) faceDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;

        for (int i = 0; i < cnt; i++)
        {
            var col = overlapBuffer[i];
            if (!col) continue;

            var t = col.GetComponent<JamipEnemyTarget>();
            if (t == null || !predicate(t)) continue;

            Vector2Int enemyGrid = GetGridFromWorld(col.transform.position);
            if (!IsOrthogonallyAdjacent(currentGridPosition, enemyGrid)) continue;

            Vector2 toEnemy = (col.transform.position - transform.position).normalized;
            float dirScore = Vector2.Dot(faceDir, toEnemy);
            float dist = Vector2.Distance(transform.position, col.transform.position);
            float score = dirScore * 2f + (-dist);

            if (score > bestScore)
            {
                bestScore = score;
                chosen = t;
            }
        }

        if (chosen != null)
            action(chosen);
    }

    // 접속 시작/해제
    void BeginConnect(JamipEnemyTarget target)
    {
        if (target == null) return;

        target.OnConnected();

        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (originalSprite == null) originalSprite = spriteRenderer.sprite;

        var enemySR = target.GetComponent<SpriteRenderer>();
        if (enemySR != null) spriteRenderer.sprite = enemySR.sprite;

        connectedTarget = target;

        // 타겟별 스텝 한정(0이면 플레이어 기본값 사용)
        int perTargetLimit = (target.enemyData != null && target.enemyData.connectStepLimit > 0)
            ? target.enemyData.connectStepLimit
            : connectMaxSteps;

        connectStepsLeft = Mathf.Max(1, perTargetLimit);
    }

    void EndConnect()
    {
        if (connectedTarget != null)
        {
            connectedTarget.OnReleased();
            connectedTarget = null;
        }
        if (spriteRenderer && originalSprite != null)
            spriteRenderer.sprite = originalSprite;
    }

    // ========================= 습격 =========================
    void HandleAssault()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            DoAssaultAdjacent();
    }

    void DoAssaultAdjacent()
    {
        int cnt = Physics2D.OverlapCircle(transform.position, tileWidth * 1.1f, enemyFilter, overlapBuffer);
        if (cnt == 0) return;

        JamipEnemyTarget chosen = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < cnt; i++)
        {
            var t = overlapBuffer[i].GetComponent<JamipEnemyTarget>();
            if (t == null || !t.CanBeAttacked) continue;

            Vector2Int enemyGrid = GetGridFromWorld(overlapBuffer[i].transform.position);
            if (IsOrthogonallyAdjacent(currentGridPosition, enemyGrid))
            {
                float dist = Vector2.Distance(transform.position, overlapBuffer[i].transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    chosen = t;
                }
            }
        }

        if (chosen == null) return;

        Vector3 dir = (chosen.transform.position - transform.position).normalized;
        if (dir.x != 0) transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

        chosen.OnAttacked();
        FearSelectionManager.Instance?.EnterBattle();
    }

    // ========================= 그리드 계산 유틸 =========================
    Vector2Int GetGridFromWorld(Vector3 worldPos)
    {
        bool vertical = IsVerticalScroll();
        if (vertical)
        {
            float laneRegionWidth = lanesCount * tileWidth;
            float laneOriginX = mainCamera.transform.position.x - laneRegionWidth * 0.5f + tileWidth * 0.5f;
            int gx = Mathf.RoundToInt((worldPos.x - laneOriginX) / tileWidth);
            int gy = Mathf.RoundToInt((worldPos.y - tileHeight * 0.5f) / tileHeight);
            return new Vector2Int(gx, gy);
        }
        else
        {
            float laneRegionHeight = lanesCount * tileHeight;
            float laneOriginY = mainCamera.transform.position.y - laneRegionHeight * 0.5f + tileHeight * 0.5f;
            int gy = Mathf.RoundToInt((worldPos.y - laneOriginY) / tileHeight);
            int gx = Mathf.RoundToInt((worldPos.x - tileWidth * 0.5f) / tileWidth);
            return new Vector2Int(gx, gy);
        }
    }

    static bool IsOrthogonallyAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    // ========================= 실패 판정 =========================
    void CheckFailCondition()
    {
        if (mapManager == null || mainCamera == null) return;
        var seg = mapManager.currentSegment;
        if (seg == null) return;

        Vector3 camPos = mainCamera.transform.position;
        float orthoH = 2f * mainCamera.orthographicSize;
        float orthoW = orthoH * mainCamera.aspect;

        float left = camPos.x - orthoW * 0.5f;
        float right = camPos.x + orthoW * 0.5f;
        float bottom = camPos.y - orthoH * 0.5f;
        float top = camPos.y + orthoH * 0.5f;

        Bounds b = spriteRenderer != null ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);

        float m = seg.failMargin;
        bool failed = false;

        switch (seg.direction)
        {
            case CameraAutoScroll.ScrollDirection.Right:
                failed = b.max.x < (left - m);
                break;
            case CameraAutoScroll.ScrollDirection.Left:
                failed = b.min.x > (right + m);
                break;
            case CameraAutoScroll.ScrollDirection.Up:
                failed = b.max.y < (bottom - m);
                break;
            case CameraAutoScroll.ScrollDirection.Down:
                failed = b.min.y > (top + m);
                break;
        }

        if (failed)
            OnMissionFail();
    }

    void OnMissionFail()
    {
        Debug.LogWarning("❌ 카메라 뒤로 밀렸습니다. 임무 실패!");
        OnMissionFailed?.Invoke();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!mainCamera) return;
        var camPos = mainCamera.transform.position;

        bool verticalScroll = IsVerticalScroll();
        Gizmos.color = new Color(0f, 1f, 0.7f, 0.2f);

        if (verticalScroll)
        {
            float laneRegionW = lanesCount * tileWidth;
            Gizmos.DrawCube(
                new Vector3(camPos.x, camPos.y, 0f),
                new Vector3(laneRegionW, 2f * mainCamera.orthographicSize, 0f)
            );
        }
        else
        {
            float laneRegionH = lanesCount * tileHeight;
            Gizmos.DrawCube(
                new Vector3(camPos.x, camPos.y, 0f),
                new Vector3(2f * mainCamera.orthographicSize * mainCamera.aspect, laneRegionH, 0f)
            );
        }
    }
#endif

    void ConfigureEnemyFilter()
    {
        enemyFilter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true
        };
        enemyFilter.SetLayerMask(enemyLayer);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        ConfigureEnemyFilter();
    }
#endif
}