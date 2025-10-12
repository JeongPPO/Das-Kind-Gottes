using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SchedulePanelManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject schedulePanel;
    public Button closeButton;
    public GameObject questInfoPanel;
    public TMP_Text questNameText;
    public Button questCloseButton;

    [Header("Calendar")]
    public Transform dayGridParent;
    public GameObject dayButtonPrefab;
    public TMP_Text monthText; // 상단에 월 표시용

    [Header("Data")]
    public List<ScheduleDataSO> allMonths = new List<ScheduleDataSO>();

    private ScheduleDataSO currentMonthData;
    private int currentMonthIndex = 0;
    private Button selectedDayButton;

    void Start()
    {
        closeButton.onClick.AddListener(() => schedulePanel.SetActive(false));
        questCloseButton.onClick.AddListener(() => questInfoPanel.SetActive(false));

        questInfoPanel.SetActive(false);

        // 초기 월 설정
        if (allMonths.Count > 0)
            SetMonth(0);
    }

    public void OpenSchedulePanel()
    {
        schedulePanel.SetActive(true);
        if (allMonths.Count > 0 && currentMonthData == null)
            SetMonth(0);
    }

    // 외부에서 호출할 수 있는 월 전환 함수
    public void AdvanceMonth()
    {
        if (allMonths.Count == 0) return;

        currentMonthIndex = (currentMonthIndex + 1) % allMonths.Count;
        SetMonth(currentMonthIndex);
    }

    public void SetMonth(int monthIndex)
    {
        currentMonthIndex = monthIndex;
        currentMonthData = allMonths[monthIndex];

        // 월 이름 갱신
        monthText.text = $"{currentMonthData.monthNumber}월";

        GenerateCalendar();
    }

    void GenerateCalendar()
    {
        foreach (Transform child in dayGridParent)
            Destroy(child.gameObject);

        foreach (var day in currentMonthData.days)
        {
            GameObject dayObj = Instantiate(dayButtonPrefab, dayGridParent);
            TMP_Text dayText = dayObj.GetComponentInChildren<TMP_Text>();
            dayText.text = day.dayNumber.ToString();

            Button dayBtn = dayObj.GetComponent<Button>();
            dayBtn.onClick.AddListener(() => OnDayClicked(day, dayBtn));

            Image img = dayObj.GetComponent<Image>();
            img.color = day.isMandatory ? Color.red : Color.white;
        }

        selectedDayButton = null;
    }

    void OnDayClicked(DayData day, Button clickedButton)
    {
        // 이전 선택 해제
        if (selectedDayButton != null)
        {
            var colors = selectedDayButton.colors;
            colors.normalColor = Color.white;
            selectedDayButton.colors = colors;
        }

        // 현재 선택 강조
        var newColors = clickedButton.colors;
        newColors.normalColor = Color.yellow;
        clickedButton.colors = newColors;
        selectedDayButton = clickedButton;

        // 퀘스트 정보 표시
        questNameText.text = string.IsNullOrEmpty(day.questName) ? "퀘스트 없음" : day.questName;
        questInfoPanel.SetActive(true);
    }
}