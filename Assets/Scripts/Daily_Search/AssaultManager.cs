using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AssaultManager : MonoBehaviour
{
    public GameObject assaultUIPanel;
    public Text assaultUIText; // UI로 방향키 표시
    private AssaultPatternSO currentPattern;
    private int currentKeyIndex;
    private float timer;

    public IEnumerator RunAssault(AssaultPatternSO pattern)
    {
        currentPattern = pattern;
        currentKeyIndex = 0;
        timer = pattern.timeLimit;

        assaultUIPanel.SetActive(true);
        UpdateUI();

        while (timer > 0f && currentKeyIndex < currentPattern.keySequence.Length)
        {
            timer -= Time.deltaTime;

            if (Input.GetKeyDown(currentPattern.keySequence[currentKeyIndex]))
            {
                currentKeyIndex++;
                UpdateUI();
            }

            yield return null;
        }

        if (currentKeyIndex >= currentPattern.keySequence.Length)
            Debug.Log("습격 성공!");
        else
            Debug.Log("습격 실패!");

        assaultUIPanel.SetActive(false);
        currentPattern = null;
    }

    void UpdateUI()
    {
        if (currentKeyIndex < currentPattern.keySequence.Length)
            assaultUIText.text = $"Next: {currentPattern.keySequence[currentKeyIndex]}";
        else
            assaultUIText.text = "Complete!";
    }
}