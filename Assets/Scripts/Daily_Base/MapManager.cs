using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using static Unity.Collections.Unicode;
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
            foreach (var loc in currentMap.locations)
            {
                loc.isVisited = false;
            }
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
        var runner = Object.FindFirstObjectByType<DialogueRunner>();

        // 패널 전환 로직 실행
        SwitchPanel(loc);

        if (runner == null)
        {
            Debug.LogError("DialogueRunner를 찾을 수 없습니다!");
            return;
        }

        // 1. 어떤 노드를 실행할지 결정 (방문 여부에 따른 분기)
        string nodeToRun = "";
        if (!loc.isVisited && !string.IsNullOrEmpty(loc.firstVisitYarnNode))
        {
            nodeToRun = loc.firstVisitYarnNode;
            loc.isVisited = true; // 첫 방문 처리
            Debug.Log($"[MapManager] 첫 방문입니다. 노드 실행: {nodeToRun}");
        }
        else if (!string.IsNullOrEmpty(loc.yarnNode))
        {
            nodeToRun = loc.yarnNode;
            Debug.Log($"[MapManager] 재방문입니다. 노드 실행: {nodeToRun}");
        }

        // 2. Yarn Dialogue 실행 (결정된 nodeToRun이 있을 때만)
        if (!string.IsNullOrEmpty(nodeToRun))
        {
            if (runner.IsDialogueRunning)
            {
                runner.Stop();
            }

            try
            {
                runner.StartDialogue(nodeToRun);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MapManager] 대화 시작 오류: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[MapManager] 실행할 노드가 없어 대화를 스킵합니다.");
        }

        // 3. 패널 전환 로직
        void SwitchPanel(LocationData loc)
        {
            // [중요 수정] Transform.Find 대신 이 방식을 사용하세요.
            // 비활성화된 자식까지 포함해서 이름을 검색합니다.
            Transform targetPanel = null;
            foreach (Transform child in internalPanelsParent)
            {
                if (child.name == loc.sceneName)
                {
                    targetPanel = child;
                    break;
                }
            }

            if (targetPanel != null)
            {
                // 1. 모든 자식 Panel 비활성화
                foreach (Transform child in internalPanelsParent)
                {
                    child.gameObject.SetActive(false);
                }

                // 2. 선택한 Panel 활성화 (이제 Inactive 상태였어도 확실히 켭니다)
                targetPanel.gameObject.SetActive(true);
                Debug.Log($"[MapManager] '{loc.sceneName}' 전환 성공!");

                if (loc.sceneName != "MapUI_Taria")
                {
                    InternalPanelSetup(targetPanel, loc);
                }
            }
            else
            {
                Debug.LogWarning($"[MapManager] 패널 '{loc.sceneName}'을 찾을 수 없습니다. (이름 오타 확인 필수)");
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
}