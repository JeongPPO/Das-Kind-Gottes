using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;

    [Header("공포 관련")]
    public FearData.FearType trueFear;                  // 적의 정답 공포
    public bool hasSpecialFear = false;                 // 보스 특수 공포 여부
    public FearData specialFear;                         // 보스 특수 공포 데이터 (있을 경우)

    [Header("몬스터 모드")]
    public float damageThresholdForMonsterMode; // 몬스터 모드 진입 데미지 임계치

    [Header("체력")]
    public float maxHealth;
}