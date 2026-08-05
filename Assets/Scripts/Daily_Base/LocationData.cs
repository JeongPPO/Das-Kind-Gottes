using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractableObjectData
{
    public string objectName;  // 내부 Panel 버튼 이름
    public string yarnNode;    // 클릭 시 실행할 Yarn 노드
}

[CreateAssetMenu(fileName = "LocationData", menuName = "Game/Location")]
public class LocationData : ScriptableObject
{
    [Header("ID & 표시")]
    public string id;
    public string locationName;
    public Sprite icon;

    [Header("맵 앵커(0~1)")]
    [Range(0f, 1f)] public float anchorX;
    [Range(0f, 1f)] public float anchorY;

    public Vector2 anchor => new Vector2(anchorX, anchorY);

    [Header("씬/패널 전환")]
    public string sceneName;    // 내부 Panel 이름과 맞춤
    public string firstVisitYarnNode; // 첫 방문용 노드
    public string yarnNode;           // 일반 방문용 노드 (비워두면 실행 안 함)
    public bool isVisited;            // 방문 여부 저장

    [Header("내부 상호작용")]
    public List<InteractableObjectData> interactables = new List<InteractableObjectData>();
#if UNITY_EDITOR
    private void OnEnable()
    {
        // 개발 중 Play 버튼 누를 때마다 초기화되어 편리함!
        isVisited = false;
    }
#endif

}