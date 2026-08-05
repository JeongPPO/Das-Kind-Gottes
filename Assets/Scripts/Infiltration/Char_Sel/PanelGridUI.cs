using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Infiltration;

public class PanelGridUI : MonoBehaviour
{
    [Header("Grid Root/Prefab")]
    public Transform contentParent;          // GridLayoutGroup가 붙은 Content
    public GameObject portraitPrefab;        // PortraitUI가 붙은 공통 프리팹
    public Button closeButton;               // 닫기 버튼(선택)

    // 내부 상태
    private InfiltrationLoadoutSO workingLoadout;
    private InfiltrationCharacterSelectUI ownerUI;
    private RoleType currentRole; // ★ 어떤 직군 그리드를 열었는지 저장

    private readonly List<GameObject> spawned = new List<GameObject>();

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
        gameObject.SetActive(false);
    }

    // 기존 시그니처(하위호환). 기본값은 Attacker로 둠.
    public void OpenGrid(List<InfiltrationCharacterSO> list, InfiltrationLoadoutSO loadout, InfiltrationCharacterSelectUI owner)
    {
        OpenGrid(list, loadout, owner, RoleType.Attacker);
    }

    // 직군 정보를 함께 받는 오버로드
    public void OpenGrid(List<InfiltrationCharacterSO> list, InfiltrationLoadoutSO loadout, InfiltrationCharacterSelectUI owner, RoleType role)
    {
        workingLoadout = loadout;
        ownerUI = owner;
        currentRole = role;

        BuildList(list);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        Clear();
        gameObject.SetActive(false);
    }

    void BuildList(List<InfiltrationCharacterSO> src)
    {
        Clear();
        if (src == null || portraitPrefab == null || contentParent == null) return;

        foreach (var ch in src)
        {
            var go = Object.Instantiate(portraitPrefab, contentParent);
            var pu = go.GetComponent<PortraitUI>();
            if (pu != null)
            {
                pu.BindCharacter(ch, OnCharacterClicked);
            }
            spawned.Add(go);
        }
    }

    void OnCharacterClicked(InfiltrationCharacterSO ch)
    {
        // 직군 규칙에 따라 2개 슬롯 자동 채움
        InfiltrationSlotRules.ApplyCharacterToLoadout(ch, workingLoadout);

        // ★ 대표 슬롯 갱신 요청(직군과 선택 캐릭터 전달)
        ownerUI?.OnCharacterSelected(currentRole, ch);

        // 패널 닫기
        Close();
    }

    void Clear()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i]) Object.Destroy(spawned[i]);
        }
        spawned.Clear();
    }
}
