using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardKeyPanelManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject cardKeyPanel;
    public Button closeButton;

    [Header("Character List")]
    public Transform characterContentParent;
    public GameObject characterButtonPrefab;

    [Header("CardKey Display")]
    public GameObject cardKeyDisplayPanel;
    public TMP_Text characterNameText;
    public Transform cardKeyContentParent;
    public GameObject cardKeyPrefab;
    public TMP_Text cardDescriptionText;

    private List<CharacterDataSO> characters;
    private Button selectedCharacterButton;
    private Button selectedCardKeyButton;

    void Start()
    {
        closeButton.onClick.AddListener(() => cardKeyPanel.SetActive(false));

        // GameManager에서 캐릭터 목록 가져오기
        characters = GameManager.Instance.GetEncounteredCharacters();

        foreach (var character in characters)
        {
            GameObject btnObj = Instantiate(characterButtonPrefab, characterContentParent);
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            btnText.text = character.characterName;

            Image btnIcon = btnObj.transform.Find("CharacterIcon").GetComponent<Image>();
            if (character.characterSprite != null)
                btnIcon.sprite = character.characterSprite;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnCharacterClicked(character, btn));
        }

        cardKeyDisplayPanel.SetActive(false); // 초기 숨김
    }

    void OnCharacterClicked(CharacterDataSO character, Button clickedButton)
    {
        // 이전 선택 버튼 색상 복원
        if (selectedCharacterButton != null)
        {
            var colors = selectedCharacterButton.colors;
            colors.normalColor = Color.white;
            selectedCharacterButton.colors = colors;
        }

        // 현재 버튼 선택 색상 적용, 이전에 선택된 것에 파랑 적용되는 문제 해결 필요
        var newColors = clickedButton.colors;
        newColors.normalColor = Color.yellow; // 선택 색상
        clickedButton.colors = newColors;
        selectedCharacterButton = clickedButton;

        characterNameText.text = character.characterName;

        foreach (Transform child in cardKeyContentParent)
            Destroy(child.gameObject);

        foreach (var key in character.collectedCardKeys)
        {
            GameObject keyObj = Instantiate(cardKeyPrefab, cardKeyContentParent);

            TMP_Text keyText = keyObj.transform.Find("KeyName").GetComponent<TMP_Text>();
            keyText.text = key.keyName;

            Image keyIcon = keyObj.transform.Find("KeyIcon").GetComponent<Image>();
            if (key.icon != null)
                keyIcon.sprite = key.icon;

            Button keyBtn = keyObj.GetComponent<Button>();
            keyBtn.onClick.RemoveAllListeners();
            keyBtn.onClick.AddListener(() => OnCardKeyClicked(key, keyBtn));
        }

        cardDescriptionText.text = "";
        cardKeyDisplayPanel.SetActive(true);
    }

    void OnCardKeyClicked(CardKeyDataSO key, Button clickedButton)
    {
        // 이전 선택 버튼 색상 복원
        if (selectedCardKeyButton != null)
        {
            var colors = selectedCardKeyButton.colors;
            colors.normalColor = Color.white;
            selectedCardKeyButton.colors = colors;
        }

        // 현재 버튼 선택 색상 적용
        var newColors = clickedButton.colors;
        newColors.normalColor = Color.cyan; // 선택 색상
        clickedButton.colors = newColors;
        selectedCardKeyButton = clickedButton;

        // 카드 설명 표시
        cardDescriptionText.text = key.description;
    }
}