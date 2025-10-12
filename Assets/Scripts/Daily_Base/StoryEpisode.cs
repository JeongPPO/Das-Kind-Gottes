using System;
using System.Collections.Generic;
using UnityEngine;

public enum PrereqType { EpisodeCompleted, FlagTrue, StatAtLeast }

[Serializable]
public class Prerequisite
{
    public PrereqType type = PrereqType.EpisodeCompleted;
    public string episodeId;   // EpisodeCompleted 용
    public string flagName;    // FlagTrue 용
    public string statName;    // StatAtLeast 용
    public int minValue = 0;   // StatAtLeast 용
}

[Serializable]
public class StatDelta
{
    public string statName;
    public int delta = 1;
}

[Serializable]
public class Reward
{
    public List<StatDelta> statChanges = new();
    // 아이템, 골드 등은 필요 시 여기에 추가
}

[CreateAssetMenu(fileName = "StoryEpisode", menuName = "Game/Story Episode")]
public class StoryEpisode : ScriptableObject
{
    [Header("정체성")]
    public string id;                // 예: "SL1_EP1"
    public string storylineName;     // 예: "스토리라인 1"
    public string title;             // 예: "진료실의 단서"
    [TextArea] public string summary;// 툴팁용 요약

    [Header("연결")]
    public LocationData location;    // 여기 떨어짐
    public string yarnNode;          // 실행할 Yarn 노드명

    [Header("조건")]
    public List<Prerequisite> prerequisites = new(); // 선행 조건(없어도 됨)

    [Header("보상(옵션)")]
    public Reward reward;

    [Header("옵션")]
    public bool isMainEpisode = true; // 메인/사이드 구분에 사용 가능

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            Debug.LogWarning($"{name}: Episode id가 비어있습니다.");
        if (location == null)
            Debug.LogWarning($"{name}: location이 지정되지 않았습니다.");
        if (string.IsNullOrWhiteSpace(yarnNode))
            Debug.LogWarning($"{name}: yarnNode가 비어있습니다.");
    }
#endif
}
