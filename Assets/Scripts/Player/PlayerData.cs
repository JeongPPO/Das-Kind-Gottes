using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    public string playerName;
    public Sprite portrait;

    [Header("기본 능력치")]
    public float baseMaxHP = 5f;      // 고정 기준치
    public float maxHP;           // 강화, 레벨업 등으로 늘어난 현재 기준 최대 HP
    public float currentMaxHP;        // 일시적으로 감소/증가할 수 있는 현재 전투용 최대 HP
    public float attackPower = 20f;
    public float defense = 10f;
    public float critChance = 0.1f;
    public float critMultiplier = 1.5f;

    [Header("속도 (고정)")]
    public float moveSpeed = 5f;
    public float attackSpeed = 1f;
    
    private void OnEnable()
    {
        // 처음 로딩 시 baseMaxHP로 초기화
        maxHP = baseMaxHP;
        currentMaxHP = maxHP;
    }

    public void IncreaseMaxHP(float amount)
    {
        currentMaxHP += amount;
    }

    public void ResetMaxHP()
    {
        currentMaxHP = baseMaxHP;
    }
}