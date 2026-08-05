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

    [Header("External Buttons")]
    public Button openButton;

    private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();

    void Start()
    {
        // 닫기 버튼 리스너 (람다식 사용)
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        // 처음에는 패널을 꺼둠
        if (loadPanel != null)
        {
            loadPanel.SetActive(false);
        }

        // 슬롯 생성 및 초기화 로직 (기존 코드 유지)
        RefreshSlots();
    }

    void Update()
    {
        if (loadPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    // 슬롯 정보를 최신화하는 함수를 별도로 빼두면 관리하기 편합니다.
    public void RefreshSlots()
    {
        // 이미 생성된 슬롯이 있다면 정보를 다시 불러옴
        for (int i = 0; i < slotUIs.Count; i++)
        {
            SaveData data = LoadSaveSlot(i);
            slotUIs[i].SetSlotInfo(data);
        }
    }

    public void OpenPanel()
    {
        if (loadPanel != null)
        {
            RefreshSlots(); // 열 때마다 최신 저장 목록 갱신
            loadPanel.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (loadPanel != null)
        {
            loadPanel.SetActive(false);
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

        if (data.date == "Empty Slot") return;

        // GameManager에 데이터 전달
        GameManager.Instance.SetLoadedData(data);

        // 저장된 씬 이름이 있다면 그 씬으로, 없다면 기본값(Daily)으로 이동
        string targetScene = string.IsNullOrEmpty(data.sceneName) ? "Daily" : data.sceneName;

        Debug.Log($"{targetScene} 씬으로 로딩합니다.");
        SceneManager.LoadScene(targetScene);
    }
}