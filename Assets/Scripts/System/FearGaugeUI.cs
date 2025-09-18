using UnityEngine;
using UnityEngine.UI;

public class FearGaugeUI : MonoBehaviour
{
    public Slider fearGaugeSlider;

    void Start()
    {
        if (FearGaugeManager.Instance == null)
        {
            Debug.LogError("FearGaugeManager가 씬에 없습니다!");
            enabled = false;
            return;
        }

        // 최대 게이지 값 세팅
        fearGaugeSlider.maxValue = FearGaugeManager.Instance.maxFearGauge;
    }

    void Update()
    {
        // 현재 게이지 값 UI 반영
        fearGaugeSlider.value = FearGaugeManager.Instance.fearGauge;
    }
}
