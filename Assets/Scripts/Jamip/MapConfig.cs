using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Jamip/MapConfig")]
public class MapConfig : ScriptableObject
{
    public List<MapSegment> segments = new List<MapSegment>();

#if UNITY_EDITOR
    void OnValidate()
    {
        // 에디터에서 값 바뀔 때 자동 재계산 (Manager 없이도 미리보기)
        MapSegmentLayoutUtility.RebuildSequentialLayout(segments);
    }
#endif
}

[Serializable]
public class MapSegment
{
    public string name;

    [Header("Core")]
    public CameraAutoScroll.ScrollDirection direction = CameraAutoScroll.ScrollDirection.Right;
    [Min(1)] public int progressCells = 30;
    [Min(1)] public int lanes;
    public int playerStartLane = -1; // -1 이면 중앙 자동

    [Header("Scroll / Fail")]
    public float cameraScrollSpeed = 2f;
    public float failMargin = 2f;
    public bool isFinalSegment = false;

    [Header("Connection")]
    public bool sharePivotWithPrevious = true;
    [Range(0, 5)] public int overlapCells = 1;

    [Header("Runtime (Auto)")]
    [Tooltip("자동 계산된 세그먼트 시작 그리드 (디버그 용)")]
    public Vector2Int startGrid;
    [Tooltip("자동 계산된 세그먼트 끝 그리드 (포함 범위)")]
    public Vector2Int endGrid;
    public Vector2Int playerStartGrid;
    public Vector2Int worldStartGrid;
    public Vector2Int worldEndGrid;

    [HideInInspector] public float tileSize = 1f;

    public void AutoConfigurePlayerStart()
    {
        int lane = playerStartLane;
        if (lane < 0) lane = lanes / 2;
        switch (direction)
        {
            case CameraAutoScroll.ScrollDirection.Right:
            case CameraAutoScroll.ScrollDirection.Left:
                playerStartGrid = new Vector2Int(startGrid.x, worldStartGrid.y + lane);
                break;
            case CameraAutoScroll.ScrollDirection.Up:
            case CameraAutoScroll.ScrollDirection.Down:
                playerStartGrid = new Vector2Int(worldStartGrid.x + lane, startGrid.y);
                break;
        }
    }
}

public static class MapSegmentLayoutUtility
{
    public static void RebuildSequentialLayout(List<MapSegment> segments)
    {
        if (segments == null) return;
        Vector2Int anchor = Vector2Int.zero;
        MapSegment prev = null;

        foreach (var seg in segments)
        {
            if (seg == null) continue;

            Vector2Int delta = Vector2Int.zero;

            switch (seg.direction)
            {
                case CameraAutoScroll.ScrollDirection.Right:
                    seg.startGrid = new Vector2Int(anchor.x, anchor.y);
                    seg.endGrid = new Vector2Int(anchor.x + seg.progressCells - 1, anchor.y + seg.lanes - 1);
                    delta = new Vector2Int(seg.progressCells - (seg.sharePivotWithPrevious ? seg.overlapCells : 0), 0);
                    break;
                case CameraAutoScroll.ScrollDirection.Left:
                    seg.startGrid = new Vector2Int(anchor.x, anchor.y);
                    seg.endGrid = new Vector2Int(anchor.x - (seg.progressCells - 1), anchor.y + seg.lanes - 1);
                    delta = new Vector2Int(-(seg.progressCells - (seg.sharePivotWithPrevious ? seg.overlapCells : 0)), 0);
                    break;
                case CameraAutoScroll.ScrollDirection.Up:
                    seg.startGrid = new Vector2Int(anchor.x, anchor.y);
                    seg.endGrid = new Vector2Int(anchor.x + seg.lanes - 1, anchor.y + seg.progressCells - 1);
                    delta = new Vector2Int(0, seg.progressCells - (seg.sharePivotWithPrevious ? seg.overlapCells : 0));
                    break;
                case CameraAutoScroll.ScrollDirection.Down:
                    seg.startGrid = new Vector2Int(anchor.x, anchor.y);
                    seg.endGrid = new Vector2Int(anchor.x + seg.lanes - 1, anchor.y - (seg.progressCells - 1));
                    delta = new Vector2Int(0, -(seg.progressCells - (seg.sharePivotWithPrevious ? seg.overlapCells : 0)));
                    break;
            }

            seg.worldStartGrid = new Vector2Int(
                Mathf.Min(seg.startGrid.x, seg.endGrid.x),
                Mathf.Min(seg.startGrid.y, seg.endGrid.y)
            );
            seg.worldEndGrid = new Vector2Int(
                Mathf.Max(seg.startGrid.x, seg.endGrid.x),
                Mathf.Max(seg.startGrid.y, seg.endGrid.y)
            );

            seg.AutoConfigurePlayerStart();

            anchor += delta;
            prev = seg;
        }
    }
}