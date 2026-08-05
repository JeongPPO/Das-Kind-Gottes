using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InfiltrationResultUI : MonoBehaviour
{
    public GameObject resultPanel; // 결과창 부모 패널
    public TextMeshProUGUI titleText; // "임무 실패" 등
    public Button retryButton;
    public Button exitButton;

    void Start()
    {
        resultPanel.SetActive(false);
        retryButton.onClick.AddListener(OnRetryClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    public void ShowResult(bool success)
    {
        resultPanel.SetActive(true);
        titleText.text = success ? "공포 극복 성공!" : "심연에 잠식당함...";

        // 게임 일시정지 (선택 사항)
        Time.timeScale = 0f;
    }

    void OnRetryClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnExitClicked()
    {
        Time.timeScale = 1f;
        // 일상 씬으로 돌아가는 로직
        SceneManager.LoadScene("Daily");
    }
}