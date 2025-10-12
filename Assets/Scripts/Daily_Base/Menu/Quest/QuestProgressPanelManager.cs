using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestProgressPanelManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject questProgressPanel;
    public Button closeButton;

    [Header("Quest Steps")]
    public Transform stepListContent;
    public GameObject stepItemPrefab; // Toggle + Text
    public TMP_Text stepDescriptionText;

    private QuestDataSO selectedQuest;

    public void OpenPanel(QuestDataSO quest)
    {
        selectedQuest = quest;
        GenerateStepList();
        questProgressPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        questProgressPanel.SetActive(false);
        ClearSteps();
    }

    void GenerateStepList()
    {
        ClearSteps();

        foreach (var step in selectedQuest.questSteps)
        {
            GameObject stepObj = Instantiate(stepItemPrefab, stepListContent);
            Toggle toggle = stepObj.GetComponent<Toggle>();
            TMP_Text text = stepObj.GetComponentInChildren<TMP_Text>();
            text.text = step.stepName;
            toggle.isOn = step.isCompleted;

            toggle.onValueChanged.AddListener((value) =>
            {
                step.isCompleted = value;
                stepDescriptionText.text = step.description;
            });
        }

        stepDescriptionText.text = "";
    }

    void ClearSteps()
    {
        foreach (Transform child in stepListContent)
            Destroy(child.gameObject);

        stepDescriptionText.text = "";
    }
}