using System.Collections.Generic;
using UnityEngine;
using Infiltration;

//윗부분은 배틀 초상화 관련 코드
public enum EmotionState
{
    Normal,
    Selected,
    Hit,
    Heal,
    Dialogue,
    LowHealth
}

[System.Serializable]
public struct CharacterEmotion
{
    public EmotionState state;
    public Sprite sprite;
}

//아랫부분은 배틀 입장 전
[CreateAssetMenu(fileName = "Character", menuName = "Infiltration/Character")]
public class InfiltrationCharacterSO : ScriptableObject
{
    public string characterId;
    public string displayName;
    public Sprite defaultPortrait;

    public List<CharacterEmotion> emotions;
    public Sprite portrait;
    public RoleType role;

    [Header("상태별 표정 모음")]
    

    [Tooltip("이 캐릭터가 제공하는 스킬 선택지(선택 UI에 표시용)")]
    public List<InfiltrationSkillSO> providedSkills = new List<InfiltrationSkillSO>();

    [Header("이 캐릭터의 역할 슬롯 스킬(선택 시 로드아웃에 자동 바인딩)")]
    [Tooltip("Attacker: A_Tap / Supporter: Shift / Thief: C_Hold / Healer: Space_Tap")]
    public InfiltrationSkillSO roleSlotPrimary;

    [Tooltip("Attacker: A_Hold / Supporter: S_Tap / Thief: D_Tap / Healer: Space_Hold")]
    public InfiltrationSkillSO roleSlotSecondary;

    //얘는 배틀
    public Sprite GetSprite(EmotionState state)
    {
        foreach (var emotion in emotions)
        {
            if (emotion.state == state && emotion.sprite != null)
                return emotion.sprite;
        }
        return defaultPortrait; // 해당 표정이 없으면 기본 초상화 반환
    }
}