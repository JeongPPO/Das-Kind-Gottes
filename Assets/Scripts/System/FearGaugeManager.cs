using UnityEngine;

public class FearGaugeManager : MonoBehaviour
{
    public static FearGaugeManager Instance;

    [Header("게이지 설정")]
    public float fearGauge = 0f;       // 현재 공포 게이지
    public float maxFearGauge = 100f;        // 게이지 상한
    public bool isMonsterMode = false;       // 현재 괴물화 여부

    [Header("몬스터 모드 설정")]
    public float monsterDuration = 5f;       // 괴물화 유지 시간
    private float monsterTimer;              // 타이머

    private Enemy currentEnemy;              // 현재 전투 중인 적

    // 몬스터 모드 시작 시 호출 이벤트
    public delegate void MonsterModeHandler();
    public event MonsterModeHandler OnMonsterModeStart;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetCurrentEnemy(Enemy enemy)
    {
        currentEnemy = enemy;
        fearGauge = 0f;
        isMonsterMode = false;
    }

    public void AddDamage(float damage)
    {
        if (currentEnemy == null) return;        // 적이 없으면 무시
        if (isMonsterMode) return;               // 괴물화 중이면 무시

        fearGauge += damage;

        // 적별로 설정된 데미지 임계치 확인
        if (fearGauge >= currentEnemy.damageThresholdForMonsterMode)
        {
            fearGauge = currentEnemy.damageThresholdForMonsterMode;
            StartMonsterMode();
        }
    }

    private void StartMonsterMode()
    {
        isMonsterMode = true;
        monsterTimer = monsterDuration;
        Debug.Log("괴물화 시작");

        OnMonsterModeStart?.Invoke(); // 외부에서 버프 적용 등 처리

        // 버프 효과 예시
        // playerStats.ApplyMonsterBuff();
    }

    private void Update()
    {
        if (isMonsterMode)
        {
            monsterTimer -= Time.deltaTime;
            if (monsterTimer <= 0f)
            {
                EndMonsterMode();
            }
        }
    }

    private void EndMonsterMode()
    {
        isMonsterMode = false;
        fearGauge = 0f;

        Debug.Log("괴물화 종료");

        // 게임 일시정지 후 공포 선택창 열기
        Time.timeScale = 0f;
        FearSelectionManager.Instance.EnterBattle();
    }
}
