using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        // 현재 플레이타임 가져오기
        SaveData data = new SaveData
        {
            date = DateTime.Now.ToString("yyyy-MM-dd"),
            playTime = GameManager.Instance.GetPlayTime()
        };

        // JSON 문자열로 변환
        string json = JsonUtility.ToJson(data);

        // 슬롯별 키 생성
        string key = $"SaveSlot_{slotIndex}";

        // PlayerPrefs에 저장
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();

        // UI 업데이트
        slotUIs[slotIndex].SetSlotInfo(data);

        Debug.Log($"Saved to Slot {slotIndex + 1} | PlayTime: {data.playTime}");
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

    public void OpenPanel() => savePanel.SetActive(true);
}
