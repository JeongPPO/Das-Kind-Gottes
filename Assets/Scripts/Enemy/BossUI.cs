using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    public static BossUI Instance;
    public Slider bossHpSlider;

    void Awake() => Instance = this;

    public void UpdateHPBar(float ratio)
    {
        bossHpSlider.value = ratio;
    }
}