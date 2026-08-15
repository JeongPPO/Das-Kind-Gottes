using Infiltration;
using UnityEngine;

public class BattlePortraitManager : MonoBehaviour
{
    public static BattlePortraitManager Instance { get; private set; }

    [Header("4개 슬롯 (Attacker/Supporter/Thief/Healer 각각 드래그)")]
    [SerializeField] private BattlePortraitUI[] slots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitAllSlots()
    {
        if (slots == null) return;
        foreach (var slot in slots)
        {
            if (slot != null) slot.InitFromRuntime();
        }
    }

    public BattlePortraitUI GetSlot(RoleType role)
    {
        if (slots == null) return null;
        foreach (var slot in slots)
        {
            if (slot != null && slot.RoleType == role) return slot;
        }
        return null;
    }
}