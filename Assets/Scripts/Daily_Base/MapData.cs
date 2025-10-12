using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/MapData")]
public class MapData : ScriptableObject
{
    public string mapName;
    public Sprite mapImage;         // 맵 배경 이미지
    public List<LocationData> locations;   // 이 맵에 포함된 장소들
}