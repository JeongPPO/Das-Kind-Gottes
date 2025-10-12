using UnityEngine;

[CreateAssetMenu(fileName = "CardKeyDataSO", menuName = "CardKey/CardKeyData")]
public class CardKeyDataSO : ScriptableObject
{
    public string keyName;
    [TextArea]
    public string description;
    public Sprite icon;
}