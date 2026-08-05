using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class SavePanelManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject savePanel;
    public Button closeButton;

    [Header("Slots")]
    public GameObject slotPrefab;
    public Transform contentParent;
    public int maxSlots = 10;

    private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();
    public VariableStorageBehaviour yarnVariableStorage;

    void Start()
    {
        // 슬롯 생성
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, contentParent);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
            int index = i;

            slotUI.Initialize(index, OnSlotClicked);

            SaveData data = LoadSaveSlot(i);
            slotUI.SetSlotInfo(data);

            slotUIs.Add(slotUI);
        }
    }

    void OnSlotClicked(int slotIndex)
    {
        // 현재 씬 이름을 확인
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "TitleScene") // 타이틀 씬일 때는 '불러오기'
        {
            SaveData data = LoadSaveSlot(slotIndex);
            if (data.date != "Empty Slot")
            {
                if (yarnVariableStorage != null)
                    yarnVariableStorage.SetValue("$playerName", data.playerName);

                Debug.Log($"불러오기 성공: {data.playerName}");
                SceneManager.LoadScene("DailyScene");
            }
            else
            {
                Debug.Log("빈 슬롯입니다. 새 게임을 시작합니다.");
                SceneManager.LoadScene("DailyScene");
            }
        }
        else // 게임 중일 때는 '저장하기'
        {
            yarnVariableStorage.TryGetValue("$playerName", out string currentName);

            // SavePanelManager.cs의 OnSlotClicked 내부 저장 로직
            SaveData newData = new SaveData
            {
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                playTime = GameManager.Instance.GetPlayTime(),
                playerName = currentName,
                sceneName = SceneManager.GetActiveScene().name // 현재 씬 이름 자동 기록
            };

            string json = JsonUtility.ToJson(newData);
            PlayerPrefs.SetString($"SaveSlot_{slotIndex}", json);
            PlayerPrefs.Save();

            slotUIs[slotIndex].SetSlotInfo(newData);
            Debug.Log($"슬롯 {slotIndex + 1}에 저장 완료!");
        }
    }

    public void DeleteSlot(int slotIndex)
    {
        PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}");
        PlayerPrefs.Save();

        SaveData empty = new SaveData { date = "Empty Slot", playTime = 0f };
        slotUIs[slotIndex].SetSlotInfo(empty);

        Debug.Log($"Deleted Slot {slotIndex + 1}");
    }

    SaveData LoadSaveSlot(int slotIndex)
    {
        string key = $"SaveSlot_{slotIndex}";
        if (PlayerPrefs.HasKey(key))
            return JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(key));
        else
            return new SaveData { date = "Empty Slot", playTime = 0f };
    }

    // 불러오기
    public void LoadGameFromSlot(int slotIndex)
    {
        SaveData data = LoadSaveSlot(slotIndex);

        if (data.date != "Empty Slot")
        {
            // 3. 불러온 데이터를 Yarn 저장소에 다시 주입
            yarnVariableStorage.SetValue("$playerName", data.playerName);

            // 이후 씬 전환이나 게임 재개 로직 실행
            Debug.Log($"Loaded PlayerName: {data.playerName}");
        }
    }

    public void OpenPanel() => savePanel.SetActive(true);
}
