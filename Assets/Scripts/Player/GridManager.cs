using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("고정 배틀 영역 (월드 유닛 기준, 나중에 UI 확정되면 조정)")]
    [Tooltip("그리드가 실제로 차지할 수 있는 최대 가로/세로 크기. 칸 개수와 무관하게 절대 이 영역을 넘지 않음.")]
    public Vector2 fixedAreaSize = new Vector2(6f, 6f);

    [Header("현재 그리드 상태 (런타임에 SetupGrid로 자동 설정됨)")]
    [Tooltip("적 SO에서 주입됨. 인스펙터에서 직접 바꿔도 테스트용으로 반영됨.")]
    [Range(3, 8)] public int gridWidth = 3;
    [Range(3, 8)] public int gridHeight = 3;

    // 그리드 좌표계는 항상 (0,0) ~ (gridWidth-1, gridHeight-1) 로컬 인덱스를 사용
    public Vector2Int gridOrigin = Vector2Int.zero;

    public float CellSize { get; private set; } = 1f;
    public Vector3 GridWorldCenter { get; private set; }
    private Vector3 gridWorldBottomLeft; // 셀 (0,0)의 좌하단 월드 좌표

    private Camera battleCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        battleCamera = Camera.main;
        // 초기값 기준으로 한 번 계산해둠 (적 조우 전 기본 상태)
        RecalculateGrid();
    }

    /// 적 SO 기준으로 그리드를 새로 구성. 필드에서 적과 마주쳤을 때 호출.
    public void SetupGrid(EnemyData enemy)
    {
        SetupGrid(enemy.gridWidth, enemy.gridHeight);
    }

    /// 가로/세로 칸 수를 직접 지정해 그리드를 구성.
    public void SetupGrid(int width, int height)
    {
        gridWidth = Mathf.Clamp(width, 3, 8);
        gridHeight = Mathf.Clamp(height, 3, 8);
        RecalculateGrid();
    }

    private void RecalculateGrid()
    {
        if (battleCamera == null)
            battleCamera = Camera.main;

        // 1. 고정 영역 안에서 정사각형 셀 크기 역산 (더 빡빡한 축 기준)
        float cellByWidth = fixedAreaSize.x / gridWidth;
        float cellByHeight = fixedAreaSize.y / gridHeight;
        CellSize = Mathf.Min(cellByWidth, cellByHeight);

        // 2. 화면(카메라) 중앙을 월드 좌표로 계산 (Orthographic 기준, z=0 평면)
        GridWorldCenter = battleCamera != null
            ? new Vector3(battleCamera.transform.position.x, battleCamera.transform.position.y, 0f)
            : Vector3.zero;

        // 3. 그리드 전체 크기 계산 후, 중앙 기준으로 좌하단 좌표 산출
        float totalWidth = gridWidth * CellSize;
        float totalHeight = gridHeight * CellSize;
        gridWorldBottomLeft = GridWorldCenter - new Vector3(totalWidth * 0.5f, totalHeight * 0.5f, 0f);
    }

    /// 카메라가 옮겨졌거나 fixedAreaSize를 런타임에 바꾼 경우 강제 재계산용.
    public void RefreshGrid()
    {
        RecalculateGrid();
    }

    // 격자 좌표 → 월드 좌표 (셀 중앙 반환)
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = gridWorldBottomLeft.x + (gridPos.x - gridOrigin.x + 0.5f) * CellSize;
        float y = gridWorldBottomLeft.y + (gridPos.y - gridOrigin.y + 0.5f) * CellSize;
        return new Vector3(x, y, 0f);
    }

    // 월드 좌표 → 격자 좌표
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - gridWorldBottomLeft.x) / CellSize) + gridOrigin.x;
        int y = Mathf.FloorToInt((worldPos.y - gridWorldBottomLeft.y) / CellSize) + gridOrigin.y;
        return new Vector2Int(x, y);
    }

    // 범위 확인
    public bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= gridOrigin.x &&
               pos.x < gridOrigin.x + gridWidth &&
               pos.y >= gridOrigin.y &&
               pos.y < gridOrigin.y + gridHeight;
    }
}