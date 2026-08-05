using Infiltration;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// IInfiltrationVisible 인터페이스를 상속받습니다.
public class InfiltrationEnemyAI : MonoBehaviour, IInfiltrationVisible
{
    [Header("Detection Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Detection Logic")]
    public float detectionThreshold = 100f;
    public float detectionUpdateRate = 0.2f;
    private float currentDetection = 0f;
    private Transform _player;
    private InfiltrationGridManager _gridManager;
    private int _viewRadiusInTiles;

    [Header("Refs")]
    public Slider detectionSlider;
    public TextMeshProUGUI alertIcon;
    public SpriteRenderer enemyRenderer; // [추가] 숨길 렌더러 (Mover에 있는 것 연결)

    [Header("Chase Settings")]
    public float chaseSpeedMultiplier = 1.5f;
    public float attackDistance = 1.2f;
    public float attackCooldown = 1.5f;

    private EnemyState _currentState = EnemyState.Patrol;
    private float _nextAttackTime;
    private InfiltrationEnemyPatrol _patrol;
    private bool _isMoving = false;

    // ==========================================================
    // [IInfiltrationVisible 구현]
    // ==========================================================
    public Vector2Int GridPos => _gridManager != null ? _gridManager.WorldToGrid(transform.position) : Vector2Int.zero;

    public void SetVisible(bool visible)
    {
        // 1. 적 본체 스프라이트 숨김/표시
        if (enemyRenderer != null) enemyRenderer.enabled = visible;

        // 2. 머리 위 UI(게이지, !, ?) 숨김/표시
        if (detectionSlider != null) detectionSlider.gameObject.SetActive(visible && currentDetection > 0);
        if (alertIcon != null) alertIcon.gameObject.SetActive(visible && alertIcon.text != "");

        // 보이지 않을 때는 감지 로직을 잠시 멈추고 싶다면 여기서 조절 가능합니다.
    }

    void Awake()
    {
        // ai = GetComponentInParent<InfiltrationEnemyAI>(); // 이 부분은 자기 자신을 참조하므로 수정
        if (detectionSlider != null) detectionSlider.gameObject.SetActive(false);
        if (alertIcon != null) alertIcon.text = "";
    }
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        _gridManager = InfiltrationGridManager.Instance;

        // [중요] 시작할 때 가시성 매니저에 등록
        if (InfiltrationVisibilityManager.Instance != null)
            InfiltrationVisibilityManager.Instance.RegisterTarget(this);

        _patrol = GetComponent<InfiltrationEnemyPatrol>();

        InvokeRepeating(nameof(FindVisiblePlayer), 0f, detectionUpdateRate);

        // 초기 상태는 숨김
        SetVisible(false);
    }

    void OnDestroy()
    {
        // [중요] 파괴될 때 매니저에서 해제
        if (InfiltrationVisibilityManager.Instance != null)
            InfiltrationVisibilityManager.Instance.UnregisterTarget(this);
    }

    void Update()
    {
        UpdateUI();

        if (_currentState == EnemyState.Chase)
        {
            UpdateChaseLogic();
        }
    }

    void UpdateUI()
    {
        float gaugeNormalized = currentDetection / detectionThreshold;
        bool isChasing = _currentState == EnemyState.Chase;

        if (gaugeNormalized > 0 && !isChasing)
        {
            detectionSlider.value = gaugeNormalized;
            alertIcon.text = "?";
            alertIcon.color = Color.yellow;
        }
        else if (isChasing)
        {
            alertIcon.text = "!";
            alertIcon.color = Color.red;
        }

        // 빌보드 UI 회전 고정 로직 유지
        if (detectionSlider.transform.parent != null)
            detectionSlider.transform.parent.rotation = Quaternion.identity;
    }

    void FindVisiblePlayer()
    {
        if (_player == null) return;

        // 1. 그리드 기반 거리 체크 (최적화)
        Vector2Int enemyGrid = _gridManager.WorldToGrid(transform.position);
        Vector2Int playerGrid = _gridManager.WorldToGrid(_player.position);

        // 맨해튼 거리로 1차 필터링
        int dist = Mathf.Abs(enemyGrid.x - playerGrid.x) + Mathf.Abs(enemyGrid.y - playerGrid.y);
        if (dist > _viewRadiusInTiles)
        {
            DecreaseDetection();
            return;
        }


        // 2. 실시간 위치 기반 시야각 및 레이캐스트 (정밀 판정)
        float worldDist = Vector3.Distance(transform.position, _player.position);
        if (worldDist > viewRadius)
        {
            DecreaseDetection();
            return;
        }

        Vector3 dirToPlayer = (_player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.right, dirToPlayer); // 적이 바라보는 방향 기준

        if (angle < viewAngle * 0.5f)
        {
            // 장애물(WallMask)에 가려지는지 체크 (Linecast 사용)
            if (!Physics2D.Linecast(transform.position, _player.position, obstacleMask))
            {
                // 스텔스 레이어 체크 후 게이지 상승
                if (_player.gameObject.layer != LayerMask.NameToLayer("Player_Stealth"))
                {
                    IncreaseDetection(worldDist);
                    return;
                }
            }
        }
        DecreaseDetection();
    }

    void IncreaseDetection(float distance)
    {
        // 가까울수록 더 빨리 발견됨 (detectionUpdateRate를 곱해 프레임률 독립적으로 만듦)
        float multiplier = Mathf.Clamp01(1.0f - (distance / viewRadius));
        currentDetection += 50f * multiplier * detectionUpdateRate;

        if (currentDetection >= detectionThreshold)
        {
            OnPlayerDetected();
        }
    }

    void DecreaseDetection()
    {
        currentDetection = Mathf.Max(0, currentDetection - 20f * detectionUpdateRate);
    }

    void OnPlayerDetected()
    {
    if (_currentState == EnemyState.Chase) return;

    _currentState = EnemyState.Chase;

    // 현재 붙어있는 순찰 스크립트를 찾아 비활성화합니다.
    var patrol = GetComponent<InfiltrationEnemyPatrol>();
    if (patrol != null) 
    {
        patrol.enabled = false; 
        // 추가로, Mover가 현재 이동 중일 수 있으므로 코루틴을 멈춰줍니다.
        patrol.StopAllCoroutines(); 
    }

    Debug.Log("<color=red>[AI]</color> 발각! 순찰을 중단하고 추격을 시작합니다.");

        // 시야 감지 반복 중단 (이미 발견했으므로)
        CancelInvoke(nameof(FindVisiblePlayer));
    }

    // 기존 UpdateChaseLogic을 교체
    void UpdateChaseLogic()
    {
        if (_player == null || _isMoving) return;

        // 1. 플레이어의 현재 그리드 위치 파악
        Vector2Int playerGrid = _gridManager.WorldToGrid(_player.position);
        Vector2Int myGrid = _gridManager.WorldToGrid(transform.position);

        // 2. 이미 인접했다면 이동하지 않고 공격 시도
        float dist = Vector2Int.Distance(myGrid, playerGrid);
        if (dist <= 1.1f)
        {
            UpdateFacing(_player.position - transform.position); // 공격 전 플레이어 바라보기
            if (Time.time >= _nextAttackTime) AttackPlayer();
            return;
        }

        // 3. 플레이어 방향으로 다음 칸 결정 (간단한 경로 탐색)
        Vector2Int diff = playerGrid - myGrid;
        Vector2Int moveDir = Vector2Int.zero;

        // X축이나 Y축 중 거리가 더 먼 쪽으로 한 칸 이동
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            moveDir.x = diff.x > 0 ? 1 : -1;
        }
        else
        {
            moveDir.y = diff.y > 0 ? 1 : -1;
        }

        Vector2Int nextTile = myGrid + moveDir;

        // 4. 장애물 확인 후 이동 시작
        if (!Physics2D.OverlapCircle(_gridManager.GridToWorld(nextTile), 0.2f, obstacleMask))
        {
            StartCoroutine(MoveOneStep(nextTile));
        }
        else
        {
            // TODO: 경로가 막혔을 때의 처리 (예: 다른 방향 시도)
        }
    }

    // 새로 추가된 코루틴
    IEnumerator MoveOneStep(Vector2Int targetTile)
    {
        _isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = _gridManager.GridToWorld(targetTile);
        float elapsed = 0f;

        // 추격 속도는 이동 쿨다운 개념으로 적용
        float moveSpeed = (_patrol != null ? _patrol.moveSpeed : 2f);
        float duration = 1f / (moveSpeed * chaseSpeedMultiplier);

        // 방향 전환
        UpdateFacing(endPos - startPos);

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        _isMoving = false;
    }

    // 새로 추가된 메서드
    void UpdateFacing(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void AttackPlayer()
    {
        _nextAttackTime = Time.time + attackCooldown;
        var playerHealth = _player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // 데미지 수치는 EnemyData에서 가져오거나 직접 입력
            playerHealth.TakeDamage(0.5f);
            Debug.Log("<color=red>[AI]</color> 플레이어를 공격했습니다!");
        }
    }



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Gizmos 색상 설정
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // 시야각 Gizmos
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
    }
#endif
}