using UnityEngine;

// 스폰 시 부여되는 메타: 스폰 진행칸/레인, SO, 정리 콜백용
public class JamipEnemyRuntimeMeta : MonoBehaviour
{
    public JamipEnemyDataSO data;
    public int spawnProgress;
    public int laneIndex;
}