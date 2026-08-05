using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Infiltration;
using TMPro;

public class InfiltrationCharacterSelectUI : MonoBehaviour
{
    [Header("데이터 (직군별 리스트)")]
    public List<InfiltrationCharacterSO> attackers;
    public List<InfiltrationCharacterSO> supporters;
    public List<InfiltrationCharacterSO> thieves;
    public List<InfiltrationCharacterSO> healers;

    [Header("출력 대상 로드아웃")]
    public InfiltrationLoadoutSO workingLoadout;

    [Header("패널 참조")]
    public PanelGridUI gridPanel;

    [System.Serializable]
    public class RolePortraitSlot
    {
        public Image portraitImage;
        public TMP_Text nameText;
        [Header("기본 표시(미지정 상태)")]
        public Sprite defaultPortrait;
        public string defaultName = "-";
    }

    [Header("대표 초상화 슬롯(이미지 + 이름 텍스트)")]
    public RolePortraitSlot attackerSlot;
    public RolePortraitSlot supporterSlot;
    public RolePortraitSlot thiefSlot;
    public RolePortraitSlot healerSlot;

    void Start()
    {
        // 시작 시 기본 표시로 초기화
        ApplyDefault(attackerSlot);
        ApplyDefault(supporterSlot);
        ApplyDefault(thiefSlot);
        ApplyDefault(healerSlot);
    }

    // ▼ 대표 버튼에서 호출
    public void OpenAttackerGrid()  => gridPanel.OpenGrid(attackers,  workingLoadout, this, RoleType.Attacker);
    public void OpenSupporterGrid() => gridPanel.OpenGrid(supporters, workingLoadout, this, RoleType.Supporter);
    public void OpenThiefGrid()     => gridPanel.OpenGrid(thieves,    workingLoadout, this, RoleType.Thief);
    public void OpenHealerGrid()    => gridPanel.OpenGrid(healers,    workingLoadout, this, RoleType.Healer);

    // ▼ PanelGridUI에서 캐릭터 선택 완료 시 호출
    public void OnCharacterSelected(RoleType role, InfiltrationCharacterSO ch)
    {
        switch (role)
        {
            case RoleType.Attacker:  ApplySelection(attackerSlot, ch);  break;
            case RoleType.Supporter: ApplySelection(supporterSlot, ch); break;
            case RoleType.Thief:     ApplySelection(thiefSlot, ch);     break;
            case RoleType.Healer:    ApplySelection(healerSlot, ch);    break;
        }
    }

    void ApplySelection(RolePortraitSlot slot, InfiltrationCharacterSO ch)
    {
        if (slot == null) return;
        if (ch != null)
        {
            if (slot.portraitImage) slot.portraitImage.sprite = ch.portrait;
            if (slot.nameText) slot.nameText.text = ch.displayName;
        }
        else
        {
            ApplyDefault(slot);
        }
    }

    void ApplyDefault(RolePortraitSlot slot)
    {
        if (slot == null) return;
        if (slot.portraitImage) slot.portraitImage.sprite = slot.defaultPortrait;
        if (slot.nameText) slot.nameText.text = slot.defaultName;
    }

    public void SaveAndApply()
    {
        var holder = InfiltrationLoadoutRuntime.Instance;
        if (holder != null) holder.SetLoadout(workingLoadout);
        Debug.Log("<SelectUI> 로드아웃 저장 완료");
    }
}
