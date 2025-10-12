using UnityEngine;

[RequireComponent(typeof(JamipEnemyTarget))]
public class JamipEnemyController : MonoBehaviour
{
    [Header("Runtime")]
    public JamipEnemyDataSO data;
    public JamipEnemyMoveBehaviourSO moveBehaviour;
    public CameraAutoScroll.ScrollDirection scrollDirection;

    private JamipEnemyTarget target;
    private JamipEnemyMoveBehaviourSO.State moveState;
    private float timeSinceSpawn;

    private JamipController player;
    private JamipEnemyMoveBehaviourSO.Context ctxCache; // GC 방지용 캐시

    // 공용 아이콘 바(자식 프리팹)
    private JamipEnemyIconBar iconBar;

    void Awake()
    {
        target = GetComponent<JamipEnemyTarget>();
        iconBar = GetComponentInChildren<JamipEnemyIconBar>(true);
    }

    public void Initialize(JamipController playerController, JamipEnemyDataSO so, JamipEnemyMoveBehaviourSO move, CameraAutoScroll.ScrollDirection dir)
    {
        player = playerController;
        data = so;
        moveBehaviour = move;
        scrollDirection = dir;

        target.Initialize(so);

        // 자식의 공용 아이콘 바에 바인딩(개별 아이콘 할당 없음)
        if (!iconBar)
            iconBar = GetComponentInChildren<JamipEnemyIconBar>(true);
        if (iconBar)
            iconBar.Bind(target);

        moveState = default;
        timeSinceSpawn = 0f;

        var ctx0 = BuildContext(0f);
        if (moveBehaviour != null)
            moveBehaviour.OnSpawn(transform, ref moveState, in ctx0);
    }

    void Update()
    {
        if (moveBehaviour == null || player == null) return;

        timeSinceSpawn += Time.deltaTime;

        var ctx = BuildContext(Time.deltaTime);
        moveBehaviour.Tick(transform, ref moveState, in ctx);
    }

    void OnDestroy()
    {
        if (moveBehaviour == null || player == null) return;

        var ctx0 = BuildContext(0f);
        moveBehaviour.OnDespawn(transform, ref moveState, in ctx0);
    }

    JamipEnemyMoveBehaviourSO.Context BuildContext(float deltaTime)
    {
        ctxCache.scrollDirection = scrollDirection;
        ctxCache.deltaTime = deltaTime;
        ctxCache.timeSinceSpawn = timeSinceSpawn;
        ctxCache.tileWidth = player != null ? player.TileWidth : 1f;
        ctxCache.tileHeight = player != null ? player.TileHeight : 1f;
        ctxCache.player = player != null ? player.transform : null;
        return ctxCache;
    }
}