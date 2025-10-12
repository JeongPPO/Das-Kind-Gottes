using UnityEngine;

public abstract class JamipEnemyMoveBehaviourSO : ScriptableObject
{
    public struct Context
    {
        public CameraAutoScroll.ScrollDirection scrollDirection;
        public float deltaTime;
        public float timeSinceSpawn;
        public float tileWidth;
        public float tileHeight;
        public Transform player;
    }

    public struct State
    {
        public Vector3 spawnPosition;
        public float t;
        public int laneIndex;
        public Vector3 customData;
    }

    public virtual void OnSpawn(Transform enemy, ref State state, in Context ctx) { }
    public abstract void Tick(Transform enemy, ref State state, in Context ctx);
    public virtual void OnDespawn(Transform enemy, ref State state, in Context ctx) { }
}

[CreateAssetMenu(fileName = "Move_Static", menuName = "Jamip/EnemyMove/Static")]
public class MoveStaticSO : JamipEnemyMoveBehaviourSO
{
    public override void OnSpawn(Transform enemy, ref State state, in Context ctx)
    {
        state.spawnPosition = enemy.position;
        state.t = 0f;
    }

    public override void Tick(Transform enemy, ref State state, in Context ctx)
    {
        state.t += ctx.deltaTime;
    }
}

[CreateAssetMenu(fileName = "Move_Straight", menuName = "Jamip/EnemyMove/Straight")]
public class MoveStraightSO : JamipEnemyMoveBehaviourSO
{
    public float speed = 2f;

    public override void OnSpawn(Transform enemy, ref State state, in Context ctx)
    {
        state.spawnPosition = enemy.position;
        state.t = 0f;
    }

    public override void Tick(Transform enemy, ref State state, in Context ctx)
    {
        Vector2 dir = ctx.scrollDirection switch
        {
            CameraAutoScroll.ScrollDirection.Left => Vector2.right,
            CameraAutoScroll.ScrollDirection.Right => Vector2.left,
            CameraAutoScroll.ScrollDirection.Up => Vector2.down,
            CameraAutoScroll.ScrollDirection.Down => Vector2.up,
            _ => Vector2.left
        };
        enemy.position += (Vector3)(dir * speed * ctx.deltaTime);
        state.t += ctx.deltaTime;
    }
}

[CreateAssetMenu(fileName = "Move_Oscillate", menuName = "Jamip/EnemyMove/Oscillate")]
public class MoveOscillateSO : JamipEnemyMoveBehaviourSO
{
    public float forwardSpeed = 1.5f;
    public float amplitudeTiles = 0.5f;
    public float frequency = 2.2f;

    public override void OnSpawn(Transform enemy, ref State state, in Context ctx)
    {
        state.spawnPosition = enemy.position;
        state.t = 0f;
    }

    public override void Tick(Transform enemy, ref State state, in Context ctx)
    {
        Vector2 forward = ctx.scrollDirection switch
        {
            CameraAutoScroll.ScrollDirection.Left => Vector2.right,
            CameraAutoScroll.ScrollDirection.Right => Vector2.left,
            CameraAutoScroll.ScrollDirection.Up => Vector2.down,
            CameraAutoScroll.ScrollDirection.Down => Vector2.up,
            _ => Vector2.left
        };

        Vector2 perp = (ctx.scrollDirection == CameraAutoScroll.ScrollDirection.Left ||
                        ctx.scrollDirection == CameraAutoScroll.ScrollDirection.Right)
            ? Vector2.up
            : Vector2.right;

        float ampWorld = (ctx.scrollDirection == CameraAutoScroll.ScrollDirection.Left ||
                          ctx.scrollDirection == CameraAutoScroll.ScrollDirection.Right)
            ? ctx.tileHeight * amplitudeTiles
            : ctx.tileWidth * amplitudeTiles;

        state.t += ctx.deltaTime;
        Vector3 offset =
            (Vector3)(forward * forwardSpeed * state.t) +
            (Vector3)(perp * Mathf.Sin(state.t * frequency) * ampWorld);

        enemy.position = state.spawnPosition + offset;
    }
}

/* === Grid-based behaviours === */

// 지정된 경로(그리드 칸 목록)를 한 칸씩 이동
[CreateAssetMenu(fileName = "Move_GridPath", menuName = "Jamip/EnemyMove/GridPath")]
public class MoveGridPathSO : JamipEnemyMoveBehaviourSO
{
    [Tooltip("스폰 그리드를 기준으로 상대 좌표로 이동할지 여부")]
    public bool relativeToSpawn = true;

    [Tooltip("이동할 그리드 칸 목록(절대 또는 상대)")]
    public Vector2Int[] path = new Vector2Int[] { };

    [Tooltip("경로 끝에 도달 시 되돌아가기(PingPong). 해제 시 루프")]
    public bool pingPong = false;

    [Min(0.01f), Tooltip("한 칸 이동 간 쿨다운(초)")]
    public float stepCooldown = 0.2f;

    public override void OnSpawn(Transform enemy, ref State state, in Context ctx)
    {
        state.t = 0f;
        state.laneIndex = 0;         // path index
        state.customData = new Vector3(1f, 0f, 0f); // x=dir(±1), y=initialized
        // 스폰 시 에이전트가 스포너에서 초기화되어 있어야 함
    }

    public override void Tick(Transform enemy, ref State state, in Context ctx)
    {
        var agent = enemy.GetComponent<JamipEnemyGridAgent>();
        if (!agent) return;
        if (path == null || path.Length == 0) return;

        state.t += ctx.deltaTime;

        // 초기 1회 스냅: 경로 첫 노드가 현재 위치와 같지 않게 하려면 바로 옮기지 않고 쿨다운 후 이동
        if (state.customData.y == 0f)
        {
            state.customData.y = 1f; // initialized
            return;
        }

        if (state.t < stepCooldown) return;
        state.t = 0f;

        int idx = Mathf.Clamp(state.laneIndex, 0, path.Length - 1);
        int dir = state.customData.x >= 0f ? 1 : -1;

        // 다음 인덱스 계산
        int nextIdx = idx + dir;
        if (nextIdx >= path.Length || nextIdx < 0)
        {
            if (pingPong)
            {
                dir *= -1;
                nextIdx = Mathf.Clamp(idx + dir, 0, path.Length - 1);
                state.customData.x = dir;
            }
            else
            {
                nextIdx = (nextIdx + path.Length) % path.Length; // 루프
            }
        }

        // 목표 그리드 계산
        Vector2Int targetGrid = relativeToSpawn
            ? agent.SpawnGrid + path[nextIdx]
            : path[nextIdx];

        agent.MoveToGrid(targetGrid);
        state.laneIndex = nextIdx;
    }
}

// 진행축으로 한 칸씩 접근(간단 폴백)
[CreateAssetMenu(fileName = "Move_GridForward", menuName = "Jamip/EnemyMove/GridForward")]
public class MoveGridForwardSO : JamipEnemyMoveBehaviourSO
{
    [Min(0.01f)]
    public float stepCooldown = 0.25f;

    public override void OnSpawn(Transform enemy, ref State state, in Context ctx)
    {
        state.t = 0f;
    }

    public override void Tick(Transform enemy, ref State state, in Context ctx)
    {
        var agent = enemy.GetComponent<JamipEnemyGridAgent>();
        if (!agent) return;

        state.t += ctx.deltaTime;
        if (state.t < stepCooldown) return;
        state.t = 0f;

        Vector2Int delta = ctx.scrollDirection switch
        {
            CameraAutoScroll.ScrollDirection.Right => Vector2Int.left,  // 플레이어쪽
            CameraAutoScroll.ScrollDirection.Left => Vector2Int.right,
            CameraAutoScroll.ScrollDirection.Up => Vector2Int.down,
            CameraAutoScroll.ScrollDirection.Down => Vector2Int.up,
            _ => Vector2Int.left
        };

        agent.Step(delta);
    }
}