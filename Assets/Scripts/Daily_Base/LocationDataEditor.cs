using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LocationData))]
public class LocationDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LocationData data = (LocationData)target;

        GUILayout.Space(10);

        if (GUILayout.Button("현재 선택된 UI 버튼 위치 저장"))
        {
            if (Selection.activeGameObject != null)
            {
                RectTransform rt = Selection.activeGameObject.GetComponent<RectTransform>();
                if (rt != null && rt.parent is RectTransform parentRT)
                {
                    // 맵 이미지 RectTransform 크기 가져오기
                    float mapWidth = parentRT.rect.width;
                    float mapHeight = parentRT.rect.height;

                    // 버튼의 로컬 좌표
                    Vector2 localPos = rt.localPosition;

                    // (0~1) 정규화 좌표로 변환
                    float normalizedX = (localPos.x + mapWidth / 2f) / mapWidth;
                    float normalizedY = (localPos.y + mapHeight / 2f) / mapHeight;

                    data.anchorX = normalizedX;
                    data.anchorY = normalizedY;

                    EditorUtility.SetDirty(data); // 저장 표시
                    Debug.Log($"[LocationDataEditor] {data.id} 좌표 저장됨: ({normalizedX:F2}, {normalizedY:F2})");
                }
                else
                {
                    Debug.LogWarning("선택된 오브젝트에 RectTransform이 없거나 부모가 RectTransform이 아님!");
                }
            }
            else
            {
                Debug.LogWarning("씬에서 UI 버튼(장소)을 선택하세요!");
            }
        }
    }
}