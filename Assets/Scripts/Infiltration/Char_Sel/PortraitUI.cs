using Infiltration;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PortraitUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image portraitImage;
    public TMP_Text nameText;

    [Header("툴팁")]
    [TextArea] public string customTooltip;
    public bool showSkillSummary = true;

    [Header("모드")]
    public bool isRepresentative = false;        // 대표 초상화 모드
    public RoleType representativeRole;          // 대표 모드일 때 직군
    public InfiltrationCharacterSelectUI selectUI; // 대표 모드일 때 선택 UI 참조

    [Header("리스트 아이템 모드")]
    public InfiltrationCharacterSO character;    // 리스트 아이템 모드일 때 캐릭터
    public Action<InfiltrationCharacterSO> onClickCharacter; // 리스트 아이템 클릭 콜백

    public void BindRepresentative(RoleType role, InfiltrationCharacterSelectUI ui, Sprite icon = null, string display = null)
    {
        isRepresentative = true;
        representativeRole = role;
        selectUI = ui;
        character = null;

        if (portraitImage && icon) portraitImage.sprite = icon;
        if (nameText && !string.IsNullOrEmpty(display)) nameText.text = display;
    }

    public void BindCharacter(InfiltrationCharacterSO ch, Action<InfiltrationCharacterSO> onClick, Sprite overrideIcon = null)
    {
        isRepresentative = false;
        representativeRole = default;
        selectUI = null;

        character = ch;
        onClickCharacter = onClick;

        if (portraitImage) portraitImage.sprite = overrideIcon ? overrideIcon : ch.portrait;
        if (nameText) nameText.text = ch.displayName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string tip = BuildTooltip();
        if (!string.IsNullOrEmpty(tip))
            ToolTipUI.Instance?.Show(tip, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipUI.Instance?.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRepresentative)
        {
            if (selectUI == null) return;
            switch (representativeRole)
            {
                case RoleType.Attacker: selectUI.OpenAttackerGrid(); break;
                case RoleType.Supporter: selectUI.OpenSupporterGrid(); break;
                case RoleType.Thief: selectUI.OpenThiefGrid(); break;
                case RoleType.Healer: selectUI.OpenHealerGrid(); break;
            }
            return;
        }

        if (character != null && onClickCharacter != null)
        {
            onClickCharacter(character);
        }
    }

    string BuildTooltip()
    {
        if (!string.IsNullOrEmpty(customTooltip))
            return customTooltip;

        if (!showSkillSummary) return null;

        if (isRepresentative)
        {
            return representativeRole.ToString();
        }

        if (character != null)
        {
            string p = character.roleSlotPrimary ? character.roleSlotPrimary.displayName : "-";
            string s = character.roleSlotSecondary ? character.roleSlotSecondary.displayName : "-";
            return $"{character.displayName}\n{character.role} | 1st: {p} / 2nd: {s}";
        }
        return null;
    }
}
