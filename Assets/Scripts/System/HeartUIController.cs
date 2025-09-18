using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartUIController : MonoBehaviour
{
    [Header("하트 프리팹 및 부모")]
    public GameObject heartPrefab;
    [SerializeField] private Transform heartContainer;

    [Header("하트 스프라이트")]
    public Sprite fullHeart;
    public Sprite threeQuarterHeart;
    public Sprite halfHeart;
    public Sprite quarterHeart;
    public Sprite emptyHeart;

    [Header("UI 설정")]
    public int heartsPerRow = 20;
    public int maxHeartLimit = 40;

    private List<Image> hearts = new List<Image>();

    public void SetMaxHearts(float maxHealth)
    {
        int heartCount = Mathf.Min(Mathf.CeilToInt(maxHealth), maxHeartLimit);
        
        foreach (var heartImage in hearts)
        {
            if (heartImage != null)
                Destroy(heartImage.gameObject);
        }

        hearts.Clear();
        while (hearts.Count < heartCount)
        {
            GameObject newHeartGO = Instantiate(heartPrefab, heartContainer);
            Image heartImage = newHeartGO.GetComponent<Image>();
            if (heartImage == null)
            {
                Debug.LogError("Heart prefab에 Image 컴포넌트가 없습니다.");
            }
            else
            {
                hearts.Add(heartImage);
            }
        }
        Debug.Log($"하트 개수 설정 완료: {hearts.Count}개");

        while (hearts.Count > heartCount)
        {
            Image lastHeart = hearts[hearts.Count - 1];
            Destroy(lastHeart.gameObject);
            hearts.RemoveAt(hearts.Count - 1);
        }

        GridLayoutGroup grid = heartContainer.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = heartsPerRow;
        }
    }

    public void UpdateHearts(float currentHealth, float maxHealth)
    {
        if (hearts == null || hearts.Count == 0)
        {
            Debug.LogError("hearts 리스트가 비어있습니다. SetMaxHearts 먼저 호출하세요.");
            return;
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            float fillAmount = Mathf.Clamp(currentHealth - i, 0f, 1f);

            if (fillAmount >= 1f)
                hearts[i].sprite = fullHeart;
            else if (fillAmount >= 0.75f)
                hearts[i].sprite = threeQuarterHeart;
            else if (fillAmount >= 0.5f)
                hearts[i].sprite = halfHeart;
            else if (fillAmount >= 0.25f)
                hearts[i].sprite = quarterHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }
}
