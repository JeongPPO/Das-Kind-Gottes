using UnityEngine;
using UnityEngine.UI;

public class JamipEnemyIconBar : MonoBehaviour
{
    [Header("아이콘(고정 순서)")]
    [SerializeField] private Image iconSteal;            // 훔치기
    [SerializeField] private Image iconConnect;          // 접속
    [SerializeField] private Image iconStrong;           // 강적(공격 불가)
    [SerializeField] private Image iconAssaultRequired;  // 습격 필수

    private JamipEnemyTarget target;

    public void Bind(JamipEnemyTarget t)
    {
        if (target != null) target.StateChanged -= Refresh;
        target = t;
        if (target != null) target.StateChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (target != null) target.StateChanged -= Refresh;
    }

    public void Refresh()
    {
        if (!target) return;
        // 자리(레이아웃)는 유지하고, 해당 없을 때만 투명 처리
        SetAlpha(iconSteal, target.HasItem);
        SetAlpha(iconConnect, target.CanConnect);
        SetAlpha(iconStrong, !target.CanBeAttacked);
        SetAlpha(iconAssaultRequired, target.IsEssentialTarget);
    }

    static void SetAlpha(Image img, bool on)
    {
        if (!img) return;
        var c = img.color;
        c.a = on ? 1f : 0f; // 꺼질 때 완전 투명(빈칸처럼 보임)
        img.color = c;
        // Image.enabled는 유지(레이아웃 무너짐 방지)
    }
}