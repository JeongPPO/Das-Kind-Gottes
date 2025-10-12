using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class JamipEnemyTarget : MonoBehaviour
{
    public JamipEnemyDataSO enemyData;

    [HideInInspector] public bool isConnected = false;

    // 런타임 상태 복사본(SO 변이 금지)
    [SerializeField] private bool rtHasItem;
    [SerializeField] private bool rtCanConnect;
    [SerializeField] private bool rtIsEssential;
    [SerializeField] private bool rtInvulnerable;

    [SerializeField] private string rtItemId;
    [SerializeField] private int rtItemCount;

    public bool HasItem => rtHasItem;
    public bool CanConnect => rtCanConnect;
    public bool IsEssentialTarget => rtIsEssential;
    public bool CanBeAttacked => !rtInvulnerable;

    public event Action StateChanged;
    public event Action<string,int> ItemStolen; // itemId, count

    public void Initialize(JamipEnemyDataSO so)
    {
        enemyData = so;
        if (enemyData != null)
        {
            rtHasItem = enemyData.hasItem;
            rtCanConnect = enemyData.canConnect;
            rtIsEssential = enemyData.isEssentialTarget;
            rtInvulnerable = enemyData.isInvulnerable;

            rtItemId = enemyData.itemId;
            rtItemCount = Mathf.Max(1, enemyData.itemCount);
        }
        else
        {
            rtHasItem = rtCanConnect = rtIsEssential = false;
            rtInvulnerable = false;
            rtItemId = null;
            rtItemCount = 0;
        }
        StateChanged?.Invoke();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (enemyData != null)
        {
            rtHasItem = enemyData.hasItem;
            rtCanConnect = enemyData.canConnect;
            rtIsEssential = enemyData.isEssentialTarget;
            rtInvulnerable = enemyData.isInvulnerable;
            rtItemId = enemyData.itemId;
            rtItemCount = Mathf.Max(1, enemyData.itemCount);
        }
    }
#endif

    public void OnAttacked()
    {
        if (!CanBeAttacked)
        {
            Debug.Log($"{enemyData?.enemyName ?? "Unknown"}은(는) 습격 불가 대상입니다.");
            return;
        }
        Debug.Log($"{enemyData?.enemyName ?? "Unknown"}가 습격 당했습니다!");
        // 피해 처리/애니메이션 등
    }

    public void OnStolen()
    {
        if (!rtHasItem)
        {
            Debug.Log($"{enemyData?.enemyName ?? "Unknown"}은(는) 훔칠 아이템이 없습니다.");
            return;
        }
        rtHasItem = false;
        StateChanged?.Invoke();
        ItemStolen?.Invoke(rtItemId, rtItemCount);
        Debug.Log($"💰 {enemyData?.enemyName ?? "Unknown"}의 아이템({rtItemId} x{rtItemCount})을 훔쳤습니다!");
    }

    public void OnConnected()
    {
        if (!rtCanConnect)
        {
            Debug.Log($"{enemyData?.enemyName ?? "Unknown"}은(는) 접속 불가 대상입니다.");
            return;
        }
        isConnected = true;
        StateChanged?.Invoke();
        Debug.Log($"🔗 {enemyData?.enemyName ?? "Unknown"}에 접속했습니다!");
    }

    public void OnReleased()
    {
        isConnected = false;
        StateChanged?.Invoke();
        Debug.Log($"🔌 {enemyData?.enemyName ?? "Unknown"}에서 접속 해제.");
    }
}