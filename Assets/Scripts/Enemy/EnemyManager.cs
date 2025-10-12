using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public EnemyData CurrentEnemy;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameManager.Instance.currentBattleSession != null)
        {
            EnemyData bossData = GameManager.Instance.currentBattleSession.enemy;
            if (bossData != null)
            {
                GameObject bossInstance = Instantiate(bossData.enemyPrefab);
                // 위치, 초기화 등 추가 설정 필요 시 여기에 작성

                // Yarn 대사 시스템 연동
                // 예: DialogueRunner.Instance.StartDialogue(bossData.yarnNodeName);
            }
        }
    }
}