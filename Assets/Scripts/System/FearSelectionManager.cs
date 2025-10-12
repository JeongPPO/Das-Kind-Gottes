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
    public List<FearData> allFears;      // 기존: 기본 공포 데이터 5개
    public EnemyData currentEnemy;       // 기존: 보스전용 적 데이터

    [Header("Jamip/Assault")]
    public JamipEnemyDataSO jamipCurrentEnemy;
    // 주석처리: 잠입 씬에서만 사용, 기존 currentEnemy와 별도 관리, jamipCurrentEnemy.fearOptions로 5대 공포 선택 버튼 바인딩, jamipCurrentEnemy.clueHint, 이름, 직급, 가족관계 등 좌측 힌트 UI 참조 가능

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

        // ================= 기존 로직: 보스전용 =================
        for (int i = 0; i < baseFearButtons.Length; i++)
        {
            int idx = i;
            baseFearButtons[i].onClick.RemoveAllListeners();
            baseFearButtons[i].onClick.AddListener(() => SelectFear(allFears[idx]));
            baseFearButtons[i].gameObject.SetActive(true);
        }

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

        // ================== 잠입/습격용 ==================
        if (jamipCurrentEnemy != null)
        {
            // 버튼에 적별 공포 옵션 바인딩
            for (int i = 0; i < baseFearButtons.Length; i++)
            {
                if (i >= jamipCurrentEnemy.fearOptions.Length)
                {
                    baseFearButtons[i].gameObject.SetActive(false);
                    continue;
                }

                int idx = i;
                baseFearButtons[i].onClick.RemoveAllListeners();
                baseFearButtons[i].onClick.AddListener(() => SelectFear(jamipCurrentEnemy.fearOptions[idx]));
                baseFearButtons[i].gameObject.SetActive(true);
            }

            // 좌측 단서창 UI에 jamipCurrentEnemy.clueHint, 이름, 직급, 가족관계 등 표시 가능
        }
    }

    private void SelectFear(FearData fear)
    {
        Debug.Log($"선택한 공포: {fear.fearName}");
        ApplyFearEffect(fear);
        CloseFearPanel();
    }

    private void OnExtraFearButtonClicked()
    {
        cardKeyPanel.SetActive(true);
        CloseFearPanel();
    }

    private void ApplyFearEffect(FearData fear)
    {
        if (currentEnemy == null)
            return;

        bool isCorrect = fear.fearType == currentEnemy.trueFear;
        if (isCorrect)
        {
            Debug.Log("팀 버프 적용");
            BattleManager.Instance.ApplyBuff(fear.fearType);
        }
        else
        {
            Debug.Log("팀 디버프 + 보스 반응");
            BattleManager.Instance.ApplyDebuff(fear.fearType);
        }

        // ================== 잠입용 추가 로직 (주석) ==================
        if (jamipCurrentEnemy != null)
        {
            bool isFearCorrect = fear.fearType == currentEnemy.trueFear;
            if (isCorrect)
            {
                Debug.Log("잠입: 팀 버프 적용");
                BattleManager.Instance.ApplyBuff(fear.fearType);
            }
            else
            {
                Debug.Log("잠입: 팀 디버프 + 적 반응");
                BattleManager.Instance.ApplyDebuff(fear.fearType);
            }
        }
    }

    private void CloseFearPanel()
    {
        fearPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}