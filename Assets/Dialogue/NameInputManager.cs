using UnityEngine;
using TMPro;
using Yarn.Unity;
using System.Collections;

public class NameInputManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private DialogueRunner dialogueRunner;

    private System.Action resumeDialogue;

    private void Start()
    {
        // 시작 시 패널 숨김
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        // 엔터 키로도 확인 가능하게 설정
        if (nameInputField != null)
            nameInputField.onSubmit.AddListener((_) => OnConfirmName());
    }

    [YarnCommand("ShowNameInputUI")]
    public IEnumerator ShowNameInputUI()
    {
        if (nameInputPanel == null || dialogueRunner == null)
        {
            Debug.LogError("NameInputManager: UI 연결을 확인하세요.");
            yield break;
        }

        // UI 켜기
        nameInputPanel.SetActive(true);

        // [중요] '아니오'를 눌러 돌아왔을 때, 이전에 쓴 이름이 남아있도록 함
        if (dialogueRunner.VariableStorage.TryGetValue("$playerName", out string currentName))
        {
            nameInputField.text = currentName;
        }
        else
        {
            nameInputField.text = ""; // 처음이면 공백
        }

        // 입력창 포커스 및 커서 끝으로 이동
        nameInputField.ActivateInputField();
        nameInputField.Select();
        nameInputField.caretPosition = nameInputField.text.Length;

        // 대기 로직
        bool isDone = false;
        resumeDialogue = () => { isDone = true; };

        yield return new WaitUntil(() => isDone);

        // 안전하게 끄기
        nameInputPanel.SetActive(false);
    }

    public void OnConfirmName()
    {
        // 패널이 켜져 있을 때만 동작
        if (!nameInputPanel.activeSelf) return;

        string enteredName = nameInputField.text;

        if (string.IsNullOrWhiteSpace(enteredName))
        {
            enteredName = "서월"; // 기본 이름
        }

        // Yarn 변수 업데이트
        dialogueRunner.VariableStorage.SetValue("$playerName", enteredName);

        // UI 끄기
        nameInputPanel.SetActive(false);

        // Yarn Spinner 대화 재개
        resumeDialogue?.Invoke();
        resumeDialogue = null;
    }
}