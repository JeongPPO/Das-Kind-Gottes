using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Storyline", menuName = "Game/Storyline")]
public class Storyline : ScriptableObject
{
    public string id;                // 예: "SL1"
    public string displayName;       // 예: "스토리라인 1"
    public List<StoryEpisode> episodes = new(); // 순서대로 배치해도 되고, 자유면 굳이 순서 의미 X

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            Debug.LogWarning($"{name}: Storyline id가 비어있습니다.");
        if (episodes == null || episodes.Count == 0)
            Debug.LogWarning($"{name}: episodes가 비었습니다.");
    }
#endif
}
