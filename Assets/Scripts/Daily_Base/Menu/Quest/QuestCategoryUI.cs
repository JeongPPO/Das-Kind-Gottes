using UnityEngine;
using UnityEngine.UI;

public class QuestCategoryUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button mainButton;
    public Button sideButton;
    public Button completedButton;

    [Header("Lists")]
    public GameObject mainQuestList;
    public GameObject sideQuestList;
    public GameObject completedQuestList;

    void Start()
    {
        mainButton.onClick.AddListener(() => ShowCategory(mainQuestList));
        sideButton.onClick.AddListener(() => ShowCategory(sideQuestList));
        completedButton.onClick.AddListener(() => ShowCategory(completedQuestList));

        // 초기 상태: 전부 닫기
        ShowCategory(null);
    }

    void ShowCategory(GameObject targetList)
    {
        mainQuestList.SetActive(targetList == mainQuestList);
        sideQuestList.SetActive(targetList == sideQuestList);
        completedQuestList.SetActive(targetList == completedQuestList);
    }
}