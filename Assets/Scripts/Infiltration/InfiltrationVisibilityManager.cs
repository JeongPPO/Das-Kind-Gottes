using UnityEngine;
using System.Collections.Generic;
using Infiltration;

public class InfiltrationVisibilityManager : MonoBehaviour
{
    public static InfiltrationVisibilityManager Instance { get; private set; }

    // 관리 대상 리스트 (HashSet은 추가/삭제가 리스트보다 빠릅니다)
    private HashSet<IInfiltrationVisible> _targets = new HashSet<IInfiltrationVisible>();

    // 서포터 이동기 등으로 인해 일시적으로 밝혀진 타일들 (좌표, 사라질 시간)
    private Dictionary<Vector2Int, float> _tempRevealedTiles = new Dictionary<Vector2Int, float>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        // 잔상 시야 타이머 관리
        UpdateTempTiles();
    }

    // [등록] 적이나 아이템이 생성될 때 호출
    public void RegisterTarget(IInfiltrationVisible target) => _targets.Add(target);

    // [해제] 오브젝트가 파괴되거나 비활성화될 때 호출
    public void UnregisterTarget(IInfiltrationVisible target) => _targets.Remove(target);

    // [핵심] 플레이어가 이동할 때 PlayerController에서 호출함
    public void UpdateAllVisibility(Vector2Int playerGrid, Vector2Int moveDir)
    {
        foreach (var target in _targets)
        {
            // 1. 기본/확장 시야 판정 (GridManager의 수학 계산 활용)
            bool isVisible = InfiltrationGridManager.Instance.IsTileVisible(playerGrid, target.GridPos, moveDir);

            // 2. 이동기 잔상 시야 판정 (만약 이 타일이 일시적으로 밝혀진 상태라면)
            if (!isVisible && _tempRevealedTiles.ContainsKey(target.GridPos))
            {
                isVisible = true;
            }

            // 3. 최종 결과 적용
            target.SetVisible(isVisible);
        }
    }

    // [부가 기능] 특정 타일들을 일정 시간 동안 강제로 밝게 만듦 (이동기용)
    public void RevealTilesTemporarily(IEnumerable<Vector2Int> tiles, float duration)
    {
        float expiry = Time.time + duration;
        foreach (var tile in tiles)
        {
            if (_tempRevealedTiles.ContainsKey(tile)) _tempRevealedTiles[tile] = expiry;
            else _tempRevealedTiles.Add(tile, expiry);
        }
    }

    private void UpdateTempTiles()
    {
        if (_tempRevealedTiles.Count == 0) return;

        // 시간이 다 된 타일 삭제
        var toRemove = new List<Vector2Int>();
        foreach (var kvp in _tempRevealedTiles)
        {
            if (Time.time > kvp.Value) toRemove.Add(kvp.Key);
        }

        foreach (var tile in toRemove) _tempRevealedTiles.Remove(tile);
    }
}