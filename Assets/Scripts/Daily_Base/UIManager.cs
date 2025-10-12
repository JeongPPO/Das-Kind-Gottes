using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hudUI;
    public GameObject dialogueUI;
    public GameObject toolTipPanel;

    [Header("Menu UI (always visible)")]
    public GameObject menuUI;
    public Button cardKeyButton;
    public Button questButton;
    public Button scheduleButton;
    public Button saveButton;
    public Button playerStatButton;

    [Header("Panels")]
    public GameObject cardKeyPanel;
    public GameObject questPanel;
    public GameObject schedulePanel;
    public GameObject savePanel;
    public GameObject playerStatPanel;

    private GameObject currentPanel;

    void Start()
    {
        // 메뉴는 항상 켜져 있음
        menuUI.SetActive(true);

        // 패널은 기본적으로 꺼져 있음
        CloseAllPanels();

        // 버튼 연결
        cardKeyButton.onClick.AddListener(() => OpenPanel(cardKeyPanel));
        questButton.onClick.AddListener(() => OpenPanel(questPanel));
        scheduleButton.onClick.AddListener(() => OpenPanel(schedulePanel));
        saveButton.onClick.AddListener(() => OpenPanel(savePanel));
        playerStatButton.onClick.AddListener(() => OpenPanel(playerStatPanel));
    }

    void OpenPanel(GameObject panel)
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);

        panel.SetActive(true);
        currentPanel = panel;
    }

    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }
    }

    void CloseAllPanels()
    {
        cardKeyPanel.SetActive(false);
        questPanel.SetActive(false);
        schedulePanel.SetActive(false);
        savePanel.SetActive(false);
        playerStatPanel.SetActive(false);
        currentPanel = null;
    }
}