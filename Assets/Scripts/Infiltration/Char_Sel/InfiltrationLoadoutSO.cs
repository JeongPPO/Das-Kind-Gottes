using UnityEngine;
using Infiltration;

[CreateAssetMenu(fileName = "Loadout", menuName = "Infiltration/Loadout")]
public class InfiltrationLoadoutSO : ScriptableObject
{
    public InfiltrationCharacterSO attacker;
    public InfiltrationCharacterSO supporter;
    public InfiltrationCharacterSO thief;
    public InfiltrationCharacterSO healer;

    public InfiltrationCharacterSO GetCharacterByRole(RoleType role)
    {
        return role switch
        {
            RoleType.Attacker => attacker,
            RoleType.Supporter => supporter,
            RoleType.Thief => thief,
            RoleType.Healer => healer,
            _ => null
        };
    }

    [Header("Slots")]
    public InfiltrationSkillSO A_Tap;
    public InfiltrationSkillSO A_Hold;
    public InfiltrationSkillSO S_Tap;
    public InfiltrationSkillSO Shift;
    public InfiltrationSkillSO C_Hold;
    public InfiltrationSkillSO D_Tap;
    public InfiltrationSkillSO Space_Tap;
    public InfiltrationSkillSO Space_Hold;
}