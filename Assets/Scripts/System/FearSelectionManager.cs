using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FearSelectionManager : MonoBehaviour
{
    public static FearSelectionManager Instance;

    [Header("UI Panels")]
    public GameObject fearPanel;         // 공포 선택 패널
    public GameObject cardKeyPanel;      // 카드키 선택 패널 (일상 씬 내 UI)

    [Header("Fear Buttons")]
    public Button[] baseFearButtons;     // 기본 5개 버튼 (씬에 고정)
    public Transform fearButtonContainer;
    public Button extraFearButton;       // 특수 공포 버튼 (씬에 배치 or 프리팹)

    [Header("Data")]
    public List<FearData> allFears;      // 기본 공포 데이터 5개
    public EnemyData currentEnemy;       // 현재 전투 중인 적 정보
    public FearData extraFearData;       // 보스 특수 공포 데이터


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnterBattle() // 공포 패널 켜기
    {
        fearPanel.SetActive(true);
        cardKeyPanel.SetActive(false);
        Time.timeScale = 0f;

        // 기본 공포 버튼 세팅
        for (int i = 0; i < baseFearButtons.Length; i++)
        {
            int idx = i;
            baseFearButtons[i].onClick.RemoveAllListeners();
            baseFearButtons[i].onClick.AddListener(() => SelectFear(allFears[idx]));
            baseFearButtons[i].gameObject.SetActive(true);
        }

        // 보스 특수 공포 버튼 세팅
        if (currentEnemy != null && currentEnemy.hasSpecialFear && extraFearData != null)
        {
            extraFearButton.onClick.RemoveAllListeners();
            extraFearButton.onClick.AddListener(OnExtraFearButtonClicked);
            extraFearButton.gameObject.SetActive(true);
        }
        else
        {
            extraFearButton.gameObject.SetActive(false);
        }
    }

    private void SelectFear(FearData fear)
    {
        Debug.Log($"선택한 공포: {fear.fearName}");
        ApplyFearEffect(fear);
        CloseFearPanel();
    }

    /// 카드키 UI 열기
    private void OnExtraFearButtonClicked()
    {
        cardKeyPanel.SetActive(true);
        fearPanel.SetActive(false);
    }

    /// 버프/디버프 처리
    private void ApplyFearEffect(FearData fear)
    {
        if (currentEnemy == null)
            return;

        bool isCorrect = fear.fearType == currentEnemy.trueFear;
        if (isCorrect)
        {
            Debug.Log("팀 버프 적용");
            BattleManager.Instance.ApplyBuff(fear);
        }
        else
        {
            Debug.Log("팀 디버프 + 보스 반응");
            BattleManager.Instance.ApplyDebuff(fear);
        }
    }

    /// 공포 선택 패널 닫기
    private void CloseFearPanel()
    {
        fearPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
