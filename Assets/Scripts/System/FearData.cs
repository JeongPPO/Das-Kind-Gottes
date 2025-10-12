using UnityEngine;

[CreateAssetMenu(fileName = "FearData", menuName = "Game/FearData")]
public class FearData : ScriptableObject
{
    public string fearName;
    [TextArea] public string description;
    public FearType fearType;

    public enum FearType
    {
        Deficiency,
        Death,
        Isolation,
        Humiliation,
        Failure
    }

}