using System.Collections.Generic;
using UnityEngine;

public class JamipEnemySpawner : MonoBehaviour
{
    [Header("References")]
    public JamipController playerController;
    public Camera mainCamera;
    private CameraAutoScroll camScroll;

    [Header("Enemy Table")]
    public List<JamipEnemyDataSO> enemyTable = new List<JamipEnemyDataSO>();
    public JamipEnemyMoveBehaviourSO defaultMoveBehaviour;

    [Header("Spawn Rules (tiles)")]
    public Vector2 spawnAheadTilesRange = new Vector2(1.5f, 2.5f);
    public float despawnBehindTiles = 1.5f;
    public float laneMinSpacingTiles = 1f;

    [Header("Spawn Rate")]
    public float spawnInterval = 2f;
    public int maxAlive = 5;

    [System.Serializable]
    public class LimitEntry { public JamipEnemyDataSO data; public int maxPerRun = 3; }
    public List<LimitEntry> perEnemyLimits = new List<LimitEntry>();

    private float spawnTimer;
    private readonly List<GameObject> alive = new List<GameObject>();
    private readonly Dictionary<int, float> laneLastSpawnProg = new Dictionary<int, float>();
    private readonly Dictionary<JamipEnemyDataSO, int> perEnemyCount = new Dictionary<JamipEnemyDataSO, int>();
    private readonly HashSet<int> occupiedProgressAtSpawn = new HashSet<int>();

    private float tileW = 1f, tileH = 1f;
    private int lanes = 3;

    private JamipEnemyMoveBehaviourSO runtimeGridFallback;

    void Start()
    {
        if (!playerController) playerController = FindFirstObjectByType<JamipController>();
        if (!mainCamera) mainCamera = Camera.main;
        if (mainCamera) camScroll = mainCamera.GetComponent<CameraAutoScroll>();

        if (defaultMoveBehaviour == null)
        {
            runtimeGridFallback = ScriptableObject.CreateInstance<MoveGridForwardSO>();
            ((MoveGridForwardSO)runtimeGridFallback).stepCooldown = 0.25f;
        }
    }

    void Update()
    {
        if (!ValidateSetup()) return;

        SyncGridParams();
        DespawnPass();

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && alive.Count < maxAlive)
        {
            TrySpawnOne();
            spawnTimer = spawnInterval;
        }
    }

    bool ValidateSetup() => playerController && mainCamera && camScroll;

    void SyncGridParams()
    {
        tileW = playerController.TileWidth;
        tileH = playerController.TileHeight;
        lanes = Mathf.Max(1, playerController.LanesCount);
    }

    void TrySpawnOne()
    {
        if (enemyTable.Count == 0) return;

        var dir = camScroll.scrollDir;
        playerController.GetVisibleProgressRange(out int minProg, out int maxProg);

        float r = Random.Range(spawnAheadTilesRange.x, spawnAheadTilesRange.y);
        int spawnProg = (dir == CameraAutoScroll.ScrollDirection.Right || dir == CameraAutoScroll.ScrollDirection.Up)
            ? Mathf.FloorToInt(maxProg + r)
            : Mathf.FloorToInt(minProg - r);

        if (occupiedProgressAtSpawn.Contains(spawnProg))
            return;

        int laneIndex = Random.Range(0, lanes);
        if (laneLastSpawnProg.TryGetValue(laneIndex, out float lastProg))
            if (Mathf.Abs(spawnProg - lastProg) < laneMinSpacingTiles) return;

        var data = enemyTable[Random.Range(0, enemyTable.Count)];
        if (data == null || data.prefab == null) return;

        int limit = GetLimitFor(data);
        if (limit > 0 && perEnemyCount.TryGetValue(data, out int used) && used >= limit)
            return;

        Vector3 spawnPos = ComputeSpawnWorldPos(dir, laneIndex, spawnProg);
        var go = Instantiate(data.prefab, spawnPos, Quaternion.identity);
        go.transform.SetParent(transform, true);
        alive.Add(go);

        // 그리드 기준 스폰 그리드 계산
        Vector2Int spawnGrid = (dir == CameraAutoScroll.ScrollDirection.Up || dir == CameraAutoScroll.ScrollDirection.Down)
            ? new Vector2Int(laneIndex, spawnProg)
            : new Vector2Int(spawnProg, laneIndex);

        // 에이전트 초기화
        var agent = go.GetComponent<JamipEnemyGridAgent>();
        if (!agent) agent = go.AddComponent<JamipEnemyGridAgent>();
        agent.Initialize(playerController, mainCamera, dir, spawnGrid);

        // 컨트롤러 초기화
        var controller = go.GetComponent<JamipEnemyController>();
        var move = (controller && controller.moveBehaviour != null) ? controller.moveBehaviour
                  : (defaultMoveBehaviour != null ? defaultMoveBehaviour : runtimeGridFallback);
        if (controller != null)
            controller.Initialize(playerController, data, move, dir);
        else
        {
            var target = go.GetComponent<JamipEnemyTarget>();
            if (target != null) target.Initialize(data);
        }

        // 메타/카운트
        var meta = go.GetComponent<JamipEnemyRuntimeMeta>();
        if (!meta) meta = go.AddComponent<JamipEnemyRuntimeMeta>();
        meta.data = data;
        meta.spawnProgress = spawnProg;
        meta.laneIndex = laneIndex;

        occupiedProgressAtSpawn.Add(spawnProg);
        laneLastSpawnProg[laneIndex] = spawnProg;
        perEnemyCount[data] = (perEnemyCount.TryGetValue(data, out used) ? used : 0) + 1;
    }

    int GetLimitFor(JamipEnemyDataSO data)
    {
        foreach (var e in perEnemyLimits)
            if (e != null && e.data == data) return Mathf.Max(0, e.maxPerRun);
        return 0;
    }

    Vector3 ComputeSpawnWorldPos(CameraAutoScroll.ScrollDirection dir, int laneIndex, int prog)
    {
        Vector3 camPos = mainCamera.transform.position;

        if (dir == CameraAutoScroll.ScrollDirection.Up || dir == CameraAutoScroll.ScrollDirection.Down)
        {
            float laneRegionW = lanes * tileW;
            float laneOriginX = camPos.x - laneRegionW * 0.5f + tileW * 0.5f;
            float x = laneOriginX + laneIndex * tileW;
            float y = prog * tileH + tileH * 0.5f;
            return new Vector3(x, y, 0f);
        }
        else
        {
            float laneRegionH = lanes * tileH;
            float laneOriginY = camPos.y - laneRegionH * 0.5f + tileH * 0.5f;
            float y = laneOriginY + laneIndex * tileH;
            float x = prog * tileW + tileW * 0.5f;
            return new Vector3(x, y, 0f);
        }
    }

    void DespawnPass()
    {
        if (alive.Count == 0) return;

        playerController.GetVisibleProgressRange(out int minProg, out int maxProg);
        var dir = camScroll.scrollDir;

        int behindThreshold = (dir == CameraAutoScroll.ScrollDirection.Right || dir == CameraAutoScroll.ScrollDirection.Up)
            ? Mathf.FloorToInt(minProg - despawnBehindTiles)
            : Mathf.FloorToInt(maxProg + despawnBehindTiles);

        for (int i = alive.Count - 1; i >= 0; i--)
        {
            var go = alive[i];
            if (!go) { alive.RemoveAt(i); continue; }

            int prog = WorldToProgress(go.transform.position, dir);
            bool shouldDespawn =
                (dir == CameraAutoScroll.ScrollDirection.Right || dir == CameraAutoScroll.ScrollDirection.Up)
                    ? prog < behindThreshold
                    : prog > behindThreshold;

            if (shouldDespawn)
            {
                OnDespawnCleanup(go);
                Destroy(go);
                alive.RemoveAt(i);
            }
        }
    }

    void OnDespawnCleanup(GameObject go)
    {
        var meta = go.GetComponent<JamipEnemyRuntimeMeta>();
        if (meta)
        {
            occupiedProgressAtSpawn.Remove(meta.spawnProgress);
            if (meta.data)
            {
                if (perEnemyCount.TryGetValue(meta.data, out int used) && used > 0)
                    perEnemyCount[meta.data] = used - 1;
            }
        }
    }

    int WorldToProgress(Vector3 pos, CameraAutoScroll.ScrollDirection dir)
    {
        if (dir == CameraAutoScroll.ScrollDirection.Up || dir == CameraAutoScroll.ScrollDirection.Down)
            return Mathf.FloorToInt(pos.y / tileH);
        else
            return Mathf.FloorToInt(pos.x / tileW);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!playerController || !mainCamera) return;
        var dir = camScroll ? camScroll.scrollDir : CameraAutoScroll.ScrollDirection.Right;

        playerController.GetVisibleProgressRange(out int minProg, out int maxProg);
        float tw = playerController ? playerController.TileWidth : 1f;
        float th = playerController ? playerController.TileHeight : 1f;

        Gizmos.color = Color.yellow;

        if (dir == CameraAutoScroll.ScrollDirection.Right || dir == CameraAutoScroll.ScrollDirection.Left)
        {
            float minX = (minProg - despawnBehindTiles) * tw;
            float maxX = (maxProg + despawnBehindTiles) * tw;
            Gizmos.DrawLine(new Vector3(minX, -100, 0), new Vector3(minX, 100, 0));
            Gizmos.DrawLine(new Vector3(maxX, -100, 0), new Vector3(maxX, 100, 0));
        }
        else
        {
            float minY = (minProg - despawnBehindTiles) * th;
            float maxY = (maxProg + despawnBehindTiles) * th;
            Gizmos.DrawLine(new Vector3(-100, minY, 0), new Vector3(100, minY, 0));
            Gizmos.DrawLine(new Vector3(-100, maxY, 0), new Vector3(100, maxY, 0));
        }
    }
#endif
}