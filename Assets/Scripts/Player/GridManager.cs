using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridOrigin = new Vector2Int(-5, -3);
    public int gridWidth = 8;
    public int gridHeight = 6;

    public static GridManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // 격자 좌표 → 월드 좌표
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x + 0.5f, gridPos.y - 0.2f, 0);
    }

    // 월드 좌표 → 격자 좌표 (선택사항)
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
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