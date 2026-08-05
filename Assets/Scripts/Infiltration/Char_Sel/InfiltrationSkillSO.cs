using UnityEngine;
using Infiltration;

[CreateAssetMenu(fileName = "Skill", menuName = "Infiltration/Skill")]
public class InfiltrationSkillSO : ScriptableObject
{
    [Header("Meta")]
    public string skillId;
    public string displayName;
    public Sprite icon;
    public RoleType role;
    public SkillType type;
    public InputSlotMask bindableSlots = InputSlotMask.All;

    [Header("Cooldown")]
    public float cooldown = 0f;

    [Header("Generic Params")]
    public int rangeTiles = 3;           // LineAttack, Dash 등
    public float damageHearts = 0.5f;    // 공격 피해
    public float healHearts = 1f;        // 회복량
    public float parryWindow = 0.18f;    // 패링 유효 시간
    public float stunSeconds = 0f;       // 군중제어
    public int knockbackTiles = 0;       // 밀쳐내기

    [Header("Conditions")]
    public bool requireStealth = false;
    public bool requireAdjacent = false;

    [Header("Visual Effects")]
    public GameObject effectPrefab;      // 실제 이펙트 프리팹
    public float effectDestroyTime = 1f; // 이펙트 자동 삭제 시간
}