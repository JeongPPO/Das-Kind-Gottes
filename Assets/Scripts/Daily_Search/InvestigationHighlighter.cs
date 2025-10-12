using UnityEngine;

public class InvestigationHighlighter : MonoBehaviour
{
    public void SetHighlight(bool active)
    {
        // FindObjectsByType 사용, 정렬 불필요 → 성능 ↑
        var objs = Object.FindObjectsByType<ImportantObject>(FindObjectsSortMode.None);
        foreach (var obj in objs)
            obj.Highlight(active);
    }
}