using System.Collections;
using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("References")]
    public SchedulePanelManager schedulePanelManager; // 스케줄 패널 연결
    public DailyPlayerController player;                   // 플레이어 오브젝트
    public GameObject dayTransitionPanel;             // 전체 화면 전환 패널
    public TMP_Text dayTransitionText;                // X월 X일 텍스트

    [Header("Start Points")]
    public Transform defaultStartPoint;               // 기본 시작 위치
    public Transform towerRoomStartPoint;            // 초반 특정 장소
    public Transform headquartersStartPoint;         // 나중 특정 장소

    [Header("Day Tracking")]
    public int currentDay = 1;
    public int currentMonth = 0;

    // 특정 퀘스트 완료 후 강제 시작 위치
    private bool forceStartAtHeadquarters = false;

    public void CompleteQuest(string questName)
    {
        // 특정 퀘스트 완료 체크
        if (questName == "특정퀘스트_이름")
            forceStartAtHeadquarters = true;

        // 하루 흐름 진행
        AdvanceDay();
    }

    private void AdvanceDay()
    {
        currentDay++;

        // 한 달 종료 체크
        var monthData = schedulePanelManager.allMonths[currentMonth];
        if (currentDay > monthData.days.Length)
        {
            currentDay = 1;
            currentMonth = (currentMonth + 1) % schedulePanelManager.allMonths.Count;
        }

        // SchedulePanel 갱신
        // 올바른 코드
        schedulePanelManager.SetMonth(currentMonth);

        // 하루 전환 화면
        StartCoroutine(DayTransitionCoroutine());
    }

    private IEnumerator DayTransitionCoroutine()
    {
        dayTransitionPanel.SetActive(true);
        dayTransitionText.text = $"{currentMonth + 1}월 {currentDay}일";

        yield return new WaitForSeconds(1.5f); // 표시 시간

        dayTransitionPanel.SetActive(false);

        // 플레이어 시작 위치 결정
        MovePlayerToNextLocation();
    }

    private void MovePlayerToNextLocation()
    {
        if (forceStartAtHeadquarters)
        {
            player.transform.position = headquartersStartPoint.position;
        }
        else
        {
            // 초기 몇 일은 탑에서 시작
            if (currentDay <= 5)
                player.transform.position = towerRoomStartPoint.position;
            else
                player.transform.position = defaultStartPoint.position;
        }
    }
}
