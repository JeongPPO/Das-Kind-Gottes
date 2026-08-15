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

    [Header("패널 전환 제어")]
    [SerializeField] private GameObject characterSelectPanel; // CharacterSelect_Panel 연결
    [SerializeField] private GameObject battleHUDPanel;

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
            if (slot.portraitImage) slot.portraitImage.sprite = ch.GetSprite(EmotionState.Selected);
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
        if (gridPanel != null) gridPanel.Close();

        if (characterSelectPanel != null)
            characterSelectPanel.SetActive(false);
        else
            gameObject.SetActive(false); // 스크립트가 선택 패널 자체에 붙어있는 경우

        // 3. [전투 UI 켜기 및 동기화]
        if (battleHUDPanel != null)
        {
            battleHUDPanel.SetActive(true);
        }

        // 4. [배틀 포트레잇 갱신] 배틀 매니저에 방금 저장된 데이터로 즉시 초기화 요청
        if (BattlePortraitManager.Instance != null)
        {
            BattlePortraitManager.Instance.InitAllSlots();
        }
    }
}
