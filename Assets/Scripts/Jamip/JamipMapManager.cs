using UnityEngine;

public class JamipMapManager : MonoBehaviour
{
    public MapConfig mapConfig;
    [HideInInspector] public MapSegment currentSegment;
    public int currentSegmentIndex = 0;

    public Transform player;
    public Camera mainCamera;
    public System.Action OnMapCleared;

    void Start()
    {
        if (mapConfig == null || mapConfig.segments.Count == 0)
        {
            Debug.LogError("MapConfig 또는 세그먼트가 없습니다!");
            enabled = false;
            return;
        }

        // 레이아웃 재계산 (에디터 OnValidate 외 안전망)
        MapSegmentLayoutUtility.RebuildSequentialLayout(mapConfig.segments);

        currentSegmentIndex = Mathf.Clamp(currentSegmentIndex, 0, mapConfig.segments.Count - 1);
        currentSegment = mapConfig.segments[currentSegmentIndex];

        PlacePlayerAtStart(currentSegment);
        ApplyCameraForSegment(currentSegment);
    }

    void Update()
    {
        if (!player || currentSegment == null) return;

        // JamipController 가 별도로 진행/레인 논리 유지하므로 여기서는 단순 종료 조건만:
        if (ReachedSegmentEndLogical())
        {
            if (currentSegment.isFinalSegment || currentSegmentIndex >= mapConfig.segments.Count - 1)
            {
                HandleMapClear();
                return;
            }
            currentSegmentIndex++;
            currentSegment = mapConfig.segments[currentSegmentIndex];
            ApplyCameraForSegment(currentSegment);
            Debug.Log($"세그먼트 전환 → {currentSegment.name}");
        }
    }

    bool ReachedSegmentEndLogical()
    {
        // 진행축 방향으로 플레이어 월드 좌표를 대략 판단 (Controller 가 더 정확하지만 간단화)
        // 마지막 타일을 지나가면 true
        switch (currentSegment.direction)
        {
            case CameraAutoScroll.ScrollDirection.Right:
                return player.position.x / currentSegment.tileSize > currentSegment.endGrid.x + 0.5f;
            case CameraAutoScroll.ScrollDirection.Left:
                return player.position.x / currentSegment.tileSize < currentSegment.endGrid.x - 0.5f;
            case CameraAutoScroll.ScrollDirection.Up:
                return player.position.y / currentSegment.tileSize > currentSegment.endGrid.y + 0.5f;
            case CameraAutoScroll.ScrollDirection.Down:
                return player.position.y / currentSegment.tileSize < currentSegment.endGrid.y - 0.5f;
        }
        return false;
    }

    void ApplyCameraForSegment(MapSegment segment)
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!mainCamera) return;

        var scroll = mainCamera.GetComponent<CameraAutoScroll>();
        if (scroll)
        {
            scroll.scrollDir = segment.direction;
            scroll.scrollSpeed = segment.cameraScrollSpeed;
            scroll.playerTransform = player;
            scroll.failMargin = segment.failMargin;
        }

        // JamipController 에 세그먼트 갱신 전달
        var controller = player ? player.GetComponent<JamipController>() : null;
        if (controller) controller.ApplySegmentSettings(segment);
    }

    void PlacePlayerAtStart(MapSegment seg)
    {
        var controller = player ? player.GetComponent<JamipController>() : null;
        if (controller)
        {
            controller.currentGridPosition = seg.playerStartGrid;
            controller.ApplySegmentSettings(seg);
            return;
        }

        float t = seg.tileSize <= 0f ? 1f : seg.tileSize;
        player.position = new Vector3(
            seg.playerStartGrid.x * t + t * 0.5f,
            seg.playerStartGrid.y * t + t * 0.5f,
            0f
        );
    }

    void HandleMapClear()
    {
        Debug.Log("✅ 전체 경로 클리어!");
        OnMapCleared?.Invoke();
        enabled = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (mapConfig == null) return;
        Color[] c = { Color.cyan, Color.yellow, Color.green, Color.magenta, Color.red };
        for (int i = 0; i < mapConfig.segments.Count; i++)
        {
            var s = mapConfig.segments[i];
            Vector3 min = new Vector3(s.worldStartGrid.x, s.worldStartGrid.y, 0f);
            Vector3 size = new Vector3(
                s.worldEndGrid.x - s.worldStartGrid.x + 1,
                s.worldEndGrid.y - s.worldStartGrid.y + 1, 0f);
            Gizmos.color = c[i % c.Length] * 0.7f;
            Gizmos.DrawWireCube(min + size * 0.5f, size);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(min + size * 0.5f, $"{s.name}\n{s.direction}");
#endif
        }
    }
#endif
}