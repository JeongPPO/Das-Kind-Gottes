using UnityEngine;

[CreateAssetMenu(fileName = "JamipEnemyData", menuName = "Jamip/EnemyDataSO")]
public class JamipEnemyDataSO : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("기본 정보")]
    public string enemyName;
    public string role;

    [Header("상태 / 특성")]
    public bool hasItem = false;
    public bool canConnect = false;
    public bool isEssentialTarget = false;
    public bool isInvulnerable = false;

    [Header("소지품(훔치기)")]
    public string itemId;
    [Min(1)] public int itemCount = 1;

    [Header("접속(타겟별 스텝 한정, 0이면 플레이어 기본값 사용)")]
    [Min(0)] public int connectStepLimit = 0;

    [Header("공포(Fears)")]
    public FearData[] fearOptions = new FearData[5];
    public FearData trueFear;
    [TextArea] public string fearHint;
}