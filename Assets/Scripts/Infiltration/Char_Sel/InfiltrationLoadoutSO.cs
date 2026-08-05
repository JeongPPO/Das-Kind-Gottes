using UnityEngine;
using Infiltration;

[CreateAssetMenu(fileName = "Loadout", menuName = "Infiltration/Loadout")]
public class InfiltrationLoadoutSO : ScriptableObject
{
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