using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/QuestData")]
public class QuestDataSO : ScriptableObject
{
    public enum QuestType
    {
        Main,
        Side,
        Completed
    }
    public string questName;
    public QuestType questType;             // Main, Side, Completed
    public List<QuestStep> questSteps;      // 단계별 진행 정보
   }

[System.Serializable]
public class QuestStep
{
    public string stepName;      // 단계 이름
    public string description;   // 단계별 설명
    public bool isCompleted;     // 체크 여부
    public string rewardDescription;        // 보상 미리보기
}