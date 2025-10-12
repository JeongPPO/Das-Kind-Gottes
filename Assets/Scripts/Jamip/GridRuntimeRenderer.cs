using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 런타임에서 굵은 선으로 격자(가시 범위) + 장애물/적 예정 칸을 Mesh 한 장으로 그려주는 렌더러.
/// (Orthographic 카메라 전제)
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class GridRuntimeRenderer : MonoBehaviour
{
    [Header("References")]
    public JamipController controller;
    public Camera targetCamera;

    [Header("Range")]
    public bool onlyVisibleWindow = true;
    public int extraAheadCells = 5;

    [Header("Line Style")]
    [Range(0.005f, 1f)] public float lineThickness = 0.08f;
    public Color lineColor = new Color(0f, 1f, 0.9f, 0.9f);
    public Color laneCenterLineColor = new Color(1f, 0.85f, 0.2f, 0.95f);

    [Header("Fill Preview")]
    public Color obstacleFillColor = new Color(1f, 0.3f, 0.2f, 0.35f);
    public Color enemyFillColor = new Color(0.4f, 0.6f, 1f, 0.35f);

    [Tooltip("장애물 예정 칸 (grid 좌표)")]
    public List<Vector2Int> obstaclePreview = new();
    [Tooltip("적 예정 칸 (grid 좌표)")]
    public List<Vector2Int> enemyPreview = new();

    [Header("Performance")]
    public float rebuildMoveThreshold = 0.25f; // 카메라 or 플레이어 이동이 이 값 이상이면 재빌드
    public bool forceEveryFrame = false;

    [Header("Order")]
    public bool drawBehindEverything = true;
    public int renderQueue = 2000; // Opaque(2000) / Transparent(3000). Transparent 로 하면 반투명 겹침 부드러움.

    Mesh _mesh;
    Material _mat;
    Vector3 _lastCamPos;
    Vector3 _lastPlayerPos;
    int _lastMinProg, _lastMaxProg;
    bool _lastVertical;

    static readonly int COLOR_PROP = Shader.PropertyToID("_Color");

    void OnValidate()
    {
        if (!controller) controller = FindFirstObjectByType<JamipController>();
        if (!targetCamera) targetCamera = Camera.main;
        extraAheadCells = Mathf.Max(0, extraAheadCells);
        lineThickness = Mathf.Max(0.0001f, lineThickness);
    }

    void OnEnable()
    {
        EnsureResources();
        RebuildNow();
    }

    void OnDisable()
    {
        if (_mesh) _mesh.Clear();
    }

    void Update()
    {
        if (!controller || !targetCamera) return;
        bool vertical = controller.IsVertical;

        controller.GetVisibleProgressRange(out int minProg, out int maxProg);
        if (onlyVisibleWindow)
        {
            // 진행 축 가시 범위 + extra
            if (vertical)
            {
                minProg = Mathf.Max(minProg, 0);
                maxProg += extraAheadCells;
            }
            else
            {
                minProg = Mathf.Max(minProg, 0);
                maxProg += extraAheadCells;
            }
        }
        else
        {
            // 세그먼트 전체 진행 경계 사용하려면 MapSegment endGrid 이용 (현재 단순화)
            var seg = controller.mapManager?.currentSegment;
            if (seg != null)
            {
                if (vertical)
                {
                    minProg = seg.startGrid.y;
                    maxProg = seg.endGrid.y;
                }
                else
                {
                    minProg = seg.startGrid.x;
                    maxProg = seg.endGrid.x;
                }
            }
        }

        Vector3 camPos = targetCamera.transform.position;
        Vector3 playerPos = controller.transform.position;

        bool needRebuild =
            forceEveryFrame ||
            vertical != _lastVertical ||
            minProg != _lastMinProg ||
            maxProg != _lastMaxProg ||
            (camPos - _lastCamPos).sqrMagnitude >= rebuildMoveThreshold * rebuildMoveThreshold ||
            (playerPos - _lastPlayerPos).sqrMagnitude >= rebuildMoveThreshold * rebuildMoveThreshold;

        if (needRebuild)
        {
            _lastVertical = vertical;
            _lastMinProg = minProg;
            _lastMaxProg = maxProg;
            _lastCamPos = camPos;
            _lastPlayerPos = playerPos;
            RebuildMesh(vertical, minProg, maxProg);
        }

        // 드로우
        if (_mesh && _mat)
        {
            // 선 + 채움 하나의 머티리얼 → 색상은 버텍스 컬러에 반영
            Graphics.DrawMesh(_mesh, Matrix4x4.identity, _mat, 0);
        }
    }

    void EnsureResources()
    {
        if (!_mesh)
        {
            _mesh = new Mesh();
            _mesh.name = "GridRuntimeRendererMesh";
            _mesh.MarkDynamic();
        }
        if (!_mat)
        {
            // 간단한 버텍스 컬러용 Built-in Unlit Shader 사용
            Shader sh = Shader.Find("Sprites/Default");
            if (!sh) sh = Shader.Find("Unlit/Color");
            _mat = new Material(sh);
            _mat.name = "GridRuntimeRendererMat";
            if (drawBehindEverything)
            {
                _mat.renderQueue = renderQueue;
            }
        }
    }

    void RebuildNow()
    {
        if (!controller) return;
        controller.GetVisibleProgressRange(out int minP, out int maxP);
        bool vertical = controller.IsVertical;
        RebuildMesh(vertical, minP, maxP);
    }

    void RebuildMesh(bool vertical, int minProg, int maxProg)
    {
        EnsureResources();
        if (!_mesh) return;

        float tileW = controller.TileWidth;
        float tileH = controller.TileHeight;
        int lanes = controller.LanesCount;

        // 레인 원점 (MoveToGrid 와 동일)
        Vector3 camPos = targetCamera.transform.position;
        float laneOriginX = 0;
        float laneOriginY = 0;
        if (vertical)
        {
            float totalLaneW = lanes * tileW;
            laneOriginX = camPos.x - totalLaneW * 0.5f + tileW * 0.5f;
        }
        else
        {
            float totalLaneH = lanes * tileH;
            laneOriginY = camPos.y - totalLaneH * 0.5f + tileH * 0.5f;
        }

        // 중복 제거 위해 “격자선” 만 생성:
        // vertical: (lanes+1)개 X세로선 + (maxProg-minProg+1)개 Y수평선
        // horizontal: 진행축 세로선 + 레인 수평선

        List<Vector3> verts = new();
        List<Color> colors = new();
        List<int> tris = new();

        // ---------- FILL (장애물/적) ----------
        AddFillTiles(vertical, obstaclePreview, laneOriginX, laneOriginY, tileW, tileH, lanes, obstacleFillColor, minProg, maxProg, verts, colors, tris);
        AddFillTiles(vertical, enemyPreview, laneOriginX, laneOriginY, tileW, tileH, lanes, enemyFillColor, minProg, maxProg, verts, colors, tris);

        float halfThick = lineThickness * 0.5f;

        // ---------- GRID LINES ----------
        if (vertical)
        {
            // 세로선 (레인 경계): lane index 0..lanes
            for (int lx = 0; lx <= lanes; lx++)
            {
                float x = (laneOriginX - tileW * 0.5f) + lx * tileW;
                float y0 = (minProg * tileH);
                float y1 = ((maxProg + 1) * tileH);
                bool centerLine = (lx == lanes / 2 + 1); // 가운데 레인 오른쪽 경계선 대신, 가운데 중심선 그릴 것 → 별도 커스텀
                Color c = lineColor;
                AddThickLine(new Vector2(x, y0), new Vector2(x, y1), halfThick, c, verts, colors, tris);
            }

            // 수평선 (진행 축 경계): prog index minProg..maxProg+1
            for (int gy = minProg; gy <= maxProg + 1; gy++)
            {
                float y = gy * tileH;
                float x0 = (laneOriginX - tileW * 0.5f);
                float x1 = x0 + lanes * tileW;
                AddThickLine(new Vector2(x0, y), new Vector2(x1, y), halfThick, lineColor, verts, colors, tris);
            }

            // 가운데 레인 중심선 (lane 중간)
            float cx = laneOriginX + (lanes / 2) * tileW;
            float cy0 = minProg * tileH;
            float cy1 = (maxProg + 1) * tileH;
            AddThickLine(new Vector2(cx, cy0), new Vector2(cx, cy1), halfThick * 0.8f, laneCenterLineColor, verts, colors, tris);
        }
        else
        {
            // 수평선 (레인 경계) lane index 0..lanes
            for (int ly = 0; ly <= lanes; ly++)
            {
                float y = (laneOriginY - tileH * 0.5f) + ly * tileH;
                float x0 = (minProg * tileW);
                float x1 = ((maxProg + 1) * tileW);
                AddThickLine(new Vector2(x0, y), new Vector2(x1, y), halfThick, lineColor, verts, colors, tris);
            }

            // 세로선 (진행 축)
            for (int gx = minProg; gx <= maxProg + 1; gx++)
            {
                float x = gx * tileW;
                float y0 = (laneOriginY - tileH * 0.5f);
                float y1 = y0 + lanes * tileH;
                AddThickLine(new Vector2(x, y0), new Vector2(x, y1), halfThick, lineColor, verts, colors, tris);
            }

            // 가운데 레인 중심선
            float cy = laneOriginY + (lanes / 2) * tileH;
            float cx0 = minProg * tileW;
            float cx1 = (maxProg + 1) * tileW;
            AddThickLine(new Vector2(cx0, cy), new Vector2(cx1, cy), halfThick * 0.8f, laneCenterLineColor, verts, colors, tris);
        }

        _mesh.Clear();
        _mesh.SetVertices(verts);
        _mesh.SetColors(colors);
        _mesh.SetTriangles(tris, 0);
        _mesh.RecalculateBounds();
    }

    void AddFillTiles(
        bool vertical,
        List<Vector2Int> tiles,
        float laneOriginX,
        float laneOriginY,
        float tileW,
        float tileH,
        int lanes,
        Color fillColor,
        int minProg,
        int maxProg,
        List<Vector3> verts,
        List<Color> colors,
        List<int> tris)
    {
        if (tiles == null || tiles.Count == 0) return;

        foreach (var g in tiles)
        {
            if (vertical)
            {
                // g.x == 레인 인덱스, g.y == 진행(Y)
                if (g.x < 0 || g.x >= lanes) continue;
                if (g.y < minProg || g.y > maxProg) continue;

                float cx = laneOriginX + g.x * tileW;
                float cy = g.y * tileH + tileH * 0.5f;
                AddQuadCentered(cx, cy, tileW, tileH, fillColor, verts, colors, tris);
            }
            else
            {
                // g.y == 레인(Y), g.x == 진행(X)
                if (g.y < 0 || g.y >= lanes) continue;
                if (g.x < minProg || g.x > maxProg) continue;

                float cx = g.x * tileW + tileW * 0.5f;
                float cy = laneOriginY + g.y * tileH;
                AddQuadCentered(cx, cy, tileW, tileH, fillColor, verts, colors, tris);
            }
        }
    }

    void AddThickLine(Vector2 a, Vector2 b, float halfThickness, Color color,
        List<Vector3> verts, List<Color> colors, List<int> tris)
    {
        // 선분 방향
        Vector2 dir = (b - a);
        float len = dir.magnitude;
        if (len <= 0.00001f) return;
        dir /= len;
        Vector2 n = new Vector2(-dir.y, dir.x) * halfThickness;

        int start = verts.Count;
        // Quad (두 삼각형)
        verts.Add(new Vector3(a.x + n.x, a.y + n.y, 0f));
        verts.Add(new Vector3(a.x - n.x, a.y - n.y, 0f));
        verts.Add(new Vector3(b.x + n.x, b.y + n.y, 0f));
        verts.Add(new Vector3(b.x - n.x, b.y - n.y, 0f));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        tris.Add(start + 0); tris.Add(start + 2); tris.Add(start + 1);
        tris.Add(start + 2); tris.Add(start + 3); tris.Add(start + 1);
    }

    void AddQuadCentered(float cx, float cy, float w, float h, Color color,
        List<Vector3> verts, List<Color> colors, List<int> tris)
    {
        int start = verts.Count;
        float hw = w * 0.5f;
        float hh = h * 0.5f;
        verts.Add(new Vector3(cx - hw, cy - hh, 0f));
        verts.Add(new Vector3(cx + hw, cy - hh, 0f));
        verts.Add(new Vector3(cx + hw, cy + hh, 0f));
        verts.Add(new Vector3(cx - hw, cy + hh, 0f));
        colors.Add(color); colors.Add(color); colors.Add(color); colors.Add(color);
        tris.Add(start + 0); tris.Add(start + 1); tris.Add(start + 2);
        tris.Add(start + 0); tris.Add(start + 2); tris.Add(start + 3);
    }
}