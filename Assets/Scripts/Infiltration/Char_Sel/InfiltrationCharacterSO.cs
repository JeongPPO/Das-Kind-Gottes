using System.Collections.Generic;
using UnityEngine;
using Infiltration;

[CreateAssetMenu(fileName = "Character", menuName = "Infiltration/Character")]
public class InfiltrationCharacterSO : ScriptableObject
{
    public string characterId;
    public string displayName;
    public Sprite portrait;
    public RoleType role;

    [Tooltip("이 캐릭터가 제공하는 스킬 선택지(선택 UI에 표시용)")]
    public List<InfiltrationSkillSO> providedSkills = new List<InfiltrationSkillSO>();

    [Header("이 캐릭터의 역할 슬롯 스킬(선택 시 로드아웃에 자동 바인딩)")]
    [Tooltip("Attacker: A_Tap / Supporter: Shift / Thief: C_Hold / Healer: Space_Tap")]
    public InfiltrationSkillSO roleSlotPrimary;

    [Tooltip("Attacker: A_Hold / Supporter: S_Tap / Thief: D_Tap / Healer: Space_Hold")]
    public InfiltrationSkillSO roleSlotSecondary;
}