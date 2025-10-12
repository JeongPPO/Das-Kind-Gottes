using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveSlotUI : MonoBehaviour
{
    public TMP_Text slotText;
    private int slotIndex;
    private Action<int> onClickAction;

    void Awake()
    {
        if (slotText == null)
            slotText = GetComponentInChildren<TMP_Text>();
    }

    public void Initialize(int index, Action<int> onClick)
    {
        slotIndex = index;
        onClickAction = onClick;
        GetComponent<Button>().onClick.AddListener(() => onClickAction?.Invoke(slotIndex));
    }

    public void SetSlotInfo(SaveData data)
    {
        TimeSpan ts = TimeSpan.FromSeconds(data.playTime);
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
        slotText.text = $"Slot {slotIndex + 1}\n{data.date}\nPlay Time: {formattedTime}";
    }
}
