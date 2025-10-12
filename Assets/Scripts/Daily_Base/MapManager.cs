using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    public RectTransform mapPanel;    // 메인 맵 Panel
    public Image mapImage;            // 맵 이미지
    public GameObject markerPrefab;   // 장소 마커 버튼 Prefab
    public Transform markerParent;    // 마커 부모 Transform

    [Header("Map Data")]
    public MapData currentMap;        // MapData 하나만 참조

    [Header("Internal Panels")]
    public Transform internalPanelsParent;   // 내부 장소 Panel 모음
    public YarnProject yarnProject;          // Yarn Dialogue Runner 참조

    void Start()
    {
        if (currentMap != null)
        {
            LoadMap(currentMap);
        }
    }

    public void LoadMap(MapData mapData)
    {
        // 배경 이미지 세팅
        mapImage.sprite = mapData.mapImage;

        // 기존 마커 삭제
        foreach (Transform child in markerParent)
            Destroy(child.gameObject);

        // 마커 새로 생성
        foreach (var loc in mapData.locations)
        {
            CreateMarker(loc, markerParent);
        }
    }

    void CreateMarker(LocationData loc, Transform parent)
    {
        GameObject marker = Instantiate(markerPrefab, parent);

        // 위치(anchor 비율 적용)
        RectTransform rt = marker.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(loc.anchorX, loc.anchorY);
        rt.anchoredPosition = Vector2.zero;

        // 아이콘 & 이름
        Image img = marker.GetComponent<Image>();
        if (img != null && loc.icon != null)
            img.sprite = loc.icon;

        Text txt = marker.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = loc.locationName;

        // 클릭 이벤트
        Button btn = marker.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnLocationClicked(loc));
        }
    }

    void OnLocationClicked(LocationData loc)
    {
        // MapPanel 끄기
        mapPanel.gameObject.SetActive(false);

        Debug.Log($"Clicked: {loc.locationName}, looking for panel: {loc.sceneName}");
        Transform internalPanel = internalPanelsParent.Find(loc.sceneName);
        if (internalPanel == null)
            Debug.LogWarning("Internal panel not found!");

        if (internalPanel != null)
        {
            // 모든 자식 Panel 비활성화 (관리 편의)
            foreach (Transform child in internalPanelsParent)
                child.gameObject.SetActive(false);

            // 선택한 Panel 활성화
            internalPanel.gameObject.SetActive(true);

            // 내부 버튼 연결
            InternalPanelSetup(internalPanel, loc);
        }
        else
        {
            Debug.LogWarning($"Internal panel '{loc.sceneName}'을 찾을 수 없습니다.");
        }

        // Yarn Dialogue 실행
        if (!string.IsNullOrEmpty(loc.yarnNode))
        {
            var runner = Object.FindFirstObjectByType<DialogueRunner>();
            if (runner != null)
                runner.StartDialogue(loc.yarnNode);
        }
    }

    void InternalPanelSetup(Transform panel, LocationData loc)
    {
        // 내부 사물 버튼과 연결
        foreach (var interactable in loc.interactables)
        {
            Transform objBtn = panel.Find(interactable.objectName);
            if (objBtn != null)
            {
                Button btn = objBtn.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        var runner = Object.FindFirstObjectByType<DialogueRunner>();
                        if (runner != null && !string.IsNullOrEmpty(interactable.yarnNode))
                        {
                            runner.StartDialogue(interactable.yarnNode);
                        }
                    });
                }
            }
        }

        // "ExitButton" 처리
        Transform exitBtn = panel.Find("ExitButton");
        if (exitBtn != null)
        {
            Button btn = exitBtn.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    panel.gameObject.SetActive(false);
                    mapPanel.gameObject.SetActive(true);
                });
            }
        }
    }
}