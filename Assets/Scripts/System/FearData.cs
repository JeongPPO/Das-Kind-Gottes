using UnityEngine;

[CreateAssetMenu(fileName = "FearData", menuName = "Game/FearData")]
public class FearData : ScriptableObject
{
    public string fearName;
    [TextArea] public string description;
    public FearType fearType;
    public float duration;             // 지속 시간 (초)
    public float damageModifier;      // 팀 데미지 버프/디버프 수치 예

    public enum FearType
    {
        Deficiency,
        Death,
        Isolation,
        Humiliation,
        Failure
    }

}