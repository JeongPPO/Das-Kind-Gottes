using UnityEngine;
using TMPro;
using Yarn.Unity;
using System.Collections;

public class NameInputManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public GameObject nameInputPanel;
    public DialogueRunner dialogueRunner;

    private System.Action resumeDialogue;

    [YarnCommand("ShowNameInputUI")]
    public IEnumerator ShowNameInputUI()
    {
        // UI 켜기
        nameInputPanel.SetActive(true);

        // 대화 일시정지 → 입력 완료될 때까지 기다리기
        bool isDone = false;

        resumeDialogue = () => { isDone = true; };

        // 대화 일시 정지처럼 보이게 만들기
        yield return new WaitUntil(() => isDone);
    }

    public void OnConfirmName()
    {
        string enteredName = nameInputField.text;

        if (string.IsNullOrWhiteSpace(enteredName))
        {
            enteredName = "서월";
        }

        dialogueRunner.VariableStorage.SetValue("$playerName", enteredName);

        nameInputPanel.SetActive(false);

        // 대화 재개
        resumeDialogue?.Invoke();
        resumeDialogue = null;
    }
}