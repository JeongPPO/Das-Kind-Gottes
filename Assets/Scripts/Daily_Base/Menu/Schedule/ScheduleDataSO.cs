using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ScheduleData", menuName = "Schedule/MonthData")]
public class ScheduleDataSO : ScriptableObject
{
    public int monthNumber; // 1~6
    public DayData[] days = new DayData[28];
}

[Serializable]
public class DayData
{
    public int dayNumber; // 1~28
    public string questName; // 해당 날짜 퀘스트
    public bool isMandatory; // 필수 이벤트 여부
}