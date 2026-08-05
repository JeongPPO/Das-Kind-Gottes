using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string date;       // 저장한 날짜 (yyyy-MM-dd)
    public float playTime;    // 누적 플레이 시간 (초 단위)
    public string sceneName; // 현재 위치한 씬 이름을 저장할 변수 추가
    public string playerName;
}

