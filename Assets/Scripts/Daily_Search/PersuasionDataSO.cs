using UnityEngine;

[CreateAssetMenu(fileName = "PersuasionData", menuName = "Daily/PersuasionData")]
public class PersuasionDataSO : ScriptableObject
{
    public string enemyName;
    public string yarnNodeName; // Yarn에서 연결할 노드
}