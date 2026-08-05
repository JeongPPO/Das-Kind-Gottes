using UnityEngine;
using Infiltration;

[RequireComponent(typeof(PlayerHealth))]
public class InfiltrationPlayerLifeManager : MonoBehaviour
{
    public InfiltrationConfigSO config;
    public Vector2Int respawnGrid;
    public System.Action OnAllLivesLost;

    private PlayerHealth health;
    private InfiltrationGridManager grid;
    private int livesLeft;
    private InfiltrationResultUI resultUI;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    void Start()
    {
        grid = InfiltrationGridManager.Instance;
        livesLeft = (config != null) ? config.lives : 3;

        if (respawnGrid == Vector2Int.zero)
            respawnGrid = grid != null ? grid.WorldToGrid(transform.position) : Vector2Int.zero;

        resultUI = Object.FindFirstObjectByType<InfiltrationResultUI>();

        OnAllLivesLost += () => {
            if (resultUI != null)
            {
                resultUI.ShowResult(false);
            }
        };
    }

    // PlayerHealth에서 호출할 사망 처리 함수
    public void HandleDeath()
    {
        livesLeft = Mathf.Max(0, livesLeft - 1);
        Debug.Log($"사망 발생. 남은 목숨: {livesLeft}");

        if (livesLeft <= 0)
        {
            Debug.LogWarning("모든 목숨 소진. 임무 실패.");
            OnAllLivesLost?.Invoke();
            // 필요 시 게임오버 UI 띄우기
            return;
        }

        // 리스폰 위치로 이동
        if (grid != null)
        {
            transform.position = grid.GridToWorld(respawnGrid);
        }

        // 체력 완전 회복 및 UI 갱신
        health.InitializeHealth(true);
    }

    // 외부 UI 등에서 남은 목숨 수를 확인하기 위한 용도
    public int GetLivesLeft() => livesLeft;
}