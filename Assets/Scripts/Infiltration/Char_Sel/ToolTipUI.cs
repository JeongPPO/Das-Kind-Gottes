using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ToolTipUI : MonoBehaviour
{
    public static ToolTipUI Instance { get; private set; }

    [Header("Refs")]
    public Canvas canvas;               // Overlay 모드 권장
    public RectTransform root;          // Tooltip 패널
    public TMP_Text text;                // 내용 텍스트
    public Vector2 screenOffset = new Vector2(12f, -12f);

    private bool visible;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (!visible || root == null) return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root.parent as RectTransform,
            Input.mousePosition,
            null,
            out localPos
        );

        root.anchoredPosition = localPos + screenOffset;
    }

    public void Show(string content, Vector3 screenPos)
    {
        if (!root || !text) return;

        text.text = content;
        root.gameObject.SetActive(true);
        visible = true;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root.parent as RectTransform,
            screenPos,
            null,
            out localPos
        );

        root.anchoredPosition = localPos + screenOffset;
    }

    public void Hide()
    {
        if (!root) return;
        root.gameObject.SetActive(false);
        visible = false;
    }
}
