using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LoadPanelManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject loadPanel;
    public Button closeButton;

    [Header("Slots")]
    public GameObject slotPrefab;
    public Transform contentParent;
    public int maxSlots = 10;

    private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();

    void Start()
    {
        closeButton.onClick.AddListener(() => loadPanel.SetActive(false));

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

    SaveData LoadSaveSlot(int slotIndex)
    {
        string key = $"SaveSlot_{slotIndex}";
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            // 빈 슬롯 처리
            return new SaveData { date = "Empty Slot", playTime = 0f };
        }
    }

    void OnSlotClicked(int slotIndex)
    {
        SaveData data = LoadSaveSlot(slotIndex);

        if (data.playTime == 0f && data.date == "Empty Slot")
        {
            Debug.Log($"Slot {slotIndex + 1} is empty, cannot load.");
            return;
        }

        GameManager.Instance.SetLoadedData(data);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Daily");
    }


    public void OpenPanel() => loadPanel.SetActive(true);
}