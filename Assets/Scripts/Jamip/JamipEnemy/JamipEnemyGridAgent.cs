using UnityEngine;

[RequireComponent(typeof(Transform))]
public class JamipEnemyGridAgent : MonoBehaviour
{
    public Vector2Int CurrentGrid { get; private set; }
    public Vector2Int SpawnGrid { get; private set; }
    public CameraAutoScroll.ScrollDirection ScrollDir { get; private set; }

    private JamipController player;
    private Camera mainCamera;

    public void Initialize(JamipController playerController, Camera cam, CameraAutoScroll.ScrollDirection dir, Vector2Int spawnGrid)
    {
        player = playerController;
        mainCamera = cam != null ? cam : Camera.main;
        ScrollDir = dir;

        SpawnGrid = spawnGrid;
        CurrentGrid = spawnGrid;
        SnapToGrid(CurrentGrid);
    }

    public void MoveToGrid(Vector2Int grid)
    {
        CurrentGrid = grid;
        SnapToGrid(CurrentGrid);
    }

    public void Step(Vector2Int delta)
    {
        MoveToGrid(CurrentGrid + delta);
    }

    public Vector3 WorldFromGrid(Vector2Int grid)
    {
        if (player == null || mainCamera == null) return transform.position;

        float tileW = player.TileWidth;
        float tileH = player.TileHeight;
        int lanes = player.LanesCount;

        Vector3 camPos = mainCamera.transform.position;
        bool vertical = (ScrollDir == CameraAutoScroll.ScrollDirection.Up || ScrollDir == CameraAutoScroll.ScrollDirection.Down);

        if (vertical)
        {
            float laneRegionW = lanes * tileW;
            float laneOriginX = camPos.x - laneRegionW * 0.5f + tileW * 0.5f;
            float x = laneOriginX + grid.x * tileW;
            float y = grid.y * tileH + tileH * 0.5f;
            return new Vector3(x, y, transform.position.z);
        }
        else
        {
            float laneRegionH = lanes * tileH;
            float laneOriginY = camPos.y - laneRegionH * 0.5f + tileH * 0.5f;
            float y = laneOriginY + grid.y * tileH;
            float x = grid.x * tileW + tileW * 0.5f;
            return new Vector3(x, y, transform.position.z);
        }
    }

    private void SnapToGrid(Vector2Int grid)
    {
        transform.position = WorldFromGrid(grid);
    }
}