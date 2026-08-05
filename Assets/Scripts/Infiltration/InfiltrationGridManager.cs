using UnityEngine;

public class InfiltrationGridManager : MonoBehaviour
{
    public InfiltrationConfigSO config;
    public static InfiltrationGridManager Instance { get; private set; }

    private float _tileSize = 1f;
    public float TileSize => _tileSize;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (config == null)
        {
            Debug.LogWarning("[InfiltrationGridManager] Config SO가 할당되지 않았습니다.");
        }
        else
        {
            _tileSize = config.tileSize;
        }
        // 기존의 _gridRect 할당 로직 삭제 -> 무한히 뻗어나가는 좌표계로 변경
    }

    // 월드 좌표 -> 그리드 좌표 변환
    public Vector2Int WorldToGrid(Vector3 world)
    {
        return new Vector2Int(Mathf.FloorToInt(world.x / _tileSize), Mathf.FloorToInt(world.y / _tileSize));
    }

    // 그리드 좌표 -> 월드 좌표 변환 (타일의 중앙)
    public Vector3 GridToWorld(Vector2Int grid)
    {
        return new Vector3((grid.x + 0.5f) * _tileSize, (grid.y + 0.5f) * _tileSize, 0f);
    }

    // [가시성 핵심 로직] 타일이 플레이어 시야에 있는지 판별
    /// <param name="playerGrid">플레이어의 현재 위치</param>
    /// <param name="targetGrid">판별할 대상(적, 아이템)의 위치</param>
    /// <param name="moveDir">플레이어의 최근 이동 방향 (정지 시 Vector2Int.zero)</param>
    public bool IsTileVisible(Vector2Int playerGrid, Vector2Int targetGrid, Vector2Int moveDir)
    {
        int dx = targetGrid.x - playerGrid.x;
        int dy = targetGrid.y - playerGrid.y;

        // 1. 기본 시야: 플레이어 중심 3x3 칸 (자신 포함 상하좌우 및 대각선 1칸 거리)
        if (Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1)
        {
            return true;
        }

        // 2. 확장 시야: 이동 방향(상하좌우)으로 3x2칸 추가 확보 (총 3x5칸)
        if (moveDir != Vector2Int.zero)
        {
            // 가로(X축) 이동 중일 때 (좌 or 우)
            if (moveDir.x != 0 && moveDir.y == 0)
            {
                // 부호가 같고(진행방향), 거리가 2~3칸이며, Y축 폭이 1칸 이내(가로 3칸 두께)일 때
                if (Mathf.Sign(dx) == Mathf.Sign(moveDir.x) && Mathf.Abs(dx) >= 2 && Mathf.Abs(dx) <= 3 && Mathf.Abs(dy) <= 1)
                {
                    return true;
                }
            }
            // 세로(Y축) 이동 중일 때 (상 or 하)
            else if (moveDir.y != 0 && moveDir.x == 0)
            {
                // 부호가 같고(진행방향), 거리가 2~3칸이며, X축 폭이 1칸 이내(세로 3칸 두께)일 때
                if (Mathf.Sign(dy) == Mathf.Sign(moveDir.y) && Mathf.Abs(dy) >= 2 && Mathf.Abs(dy) <= 3 && Mathf.Abs(dx) <= 1)
                {
                    return true;
                }
            }
        }

        // 위 조건에 해당하지 않으면 보이지 않는 칸
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 런타임이 아닐 때도 격자를 보고 싶다면 필요합니다.
        float ts = (config != null) ? config.tileSize : 1f;

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f); // 아주 연하게

        // 현재 씬 뷰(Scene View)의 카메라 위치를 기준으로 격자를 그립니다.
        // 유니티 에디터 창의 중앙 지점을 가져옵니다.
        Vector3 center = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
        Vector2Int centerGrid = WorldToGrid(center);

        // 현재 보이는 지점 주변 20칸 정도만 격자를 그림 (무한 그리드 최적화)
        int range = 20;

        for (int x = centerGrid.x - range; x <= centerGrid.x + range; x++)
        {
            Vector3 start = new Vector3(x * ts, (centerGrid.y - range) * ts, 0);
            Vector3 end = new Vector3(x * ts, (centerGrid.y + range) * ts, 0);
            Gizmos.DrawLine(start, end);
        }

        for (int y = centerGrid.y - range; y <= centerGrid.y + range; y++)
        {
            Vector3 start = new Vector3((centerGrid.x - range) * ts, y * ts, 0);
            Vector3 end = new Vector3((centerGrid.x + range) * ts, y * ts, 0);
            Gizmos.DrawLine(start, end);
        }
    }
#endif
}