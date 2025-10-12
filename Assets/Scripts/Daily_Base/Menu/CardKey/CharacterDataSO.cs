using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "CardKey/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    public string characterName;
    public Sprite characterSprite;
    public List<CardKeyDataSO> collectedCardKeys;
}