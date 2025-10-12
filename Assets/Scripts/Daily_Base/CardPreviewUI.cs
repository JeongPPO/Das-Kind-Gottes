using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardPreviewUI : MonoBehaviour
{
    [Header("Card Preview Settings")]
    public Transform cardPreviewParent;  // 미리보기 카드들이 들어갈 부모
    public GameObject cardPreviewPrefab; // 카드 미리보기 UI 프리팹 (예: Text + 작은 이미지)
    public int maxPreviewCount = 5;      // 최근 몇 개만 보여줄지

    private List<string> acquiredCards = new List<string>();

    // 새 카드 획득 시 호출
    public void AddCard(string cardTitle)
    {
        acquiredCards.Add(cardTitle);

        // 최대 개수 제한
        if (acquiredCards.Count > maxPreviewCount)
            acquiredCards.RemoveAt(0);

        RefreshPreview();
    }

    // 미리보기 영역 업데이트
    void RefreshPreview()
    {
        // 기존 미리보기 제거
        foreach (Transform child in cardPreviewParent)
            Destroy(child.gameObject);

        // 새로 생성
        foreach (var title in acquiredCards)
        {
            GameObject cardObj = Instantiate(cardPreviewPrefab, cardPreviewParent);

            Text txt = cardObj.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = title;
        }
    }
}