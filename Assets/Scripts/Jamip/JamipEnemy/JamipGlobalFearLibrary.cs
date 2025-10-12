using System.Linq;
using UnityEngine;

public static class JamipGlobalFearLibrary
{
    // Resources 폴더 기준: Resources/Fears/Fear1..Fear5 또는 Fears 폴더 내 FearData 전부
    private const string FolderPath = "Fears";
    private static FearData[] _defaultSet;

    public static FearData[] DefaultSet
    {
        get
        {
            if (_defaultSet == null)
                _defaultSet = LoadDefaultSet();
            return _defaultSet;
        }
    }

    public static bool IsReady => _defaultSet != null && _defaultSet.All(fd => fd != null);

    public static void ForceReload()
    {
        _defaultSet = LoadDefaultSet();
    }

    private static FearData[] LoadDefaultSet()
    {
        var arr = new FearData[5];

        // 1차: 명시적 이름(Fear1~Fear5)
        arr[0] = Resources.Load<FearData>($"{FolderPath}/Fear1");
        arr[1] = Resources.Load<FearData>($"{FolderPath}/Fear2");
        arr[2] = Resources.Load<FearData>($"{FolderPath}/Fear3");
        arr[3] = Resources.Load<FearData>($"{FolderPath}/Fear4");
        arr[4] = Resources.Load<FearData>($"{FolderPath}/Fear5");

        if (arr.All(a => a != null))
            return arr;

        // 2차: 폴더 내 모든 FearData를 fearType 순서대로 매핑
        var all = Resources.LoadAll<FearData>(FolderPath);
        if (all != null && all.Length > 0)
        {
            // FearType 순서: Deficiency, Death, Isolation, Humiliation, Failure
            foreach (var fd in all)
            {
                switch (fd.fearType)
                {
                    case FearData.FearType.Deficiency: arr[0] = arr[0] ?? fd; break;
                    case FearData.FearType.Death: arr[1] = arr[1] ?? fd; break;
                    case FearData.FearType.Isolation: arr[2] = arr[2] ?? fd; break;
                    case FearData.FearType.Humiliation: arr[3] = arr[3] ?? fd; break;
                    case FearData.FearType.Failure: arr[4] = arr[4] ?? fd; break;
                }
            }
        }

        // 최종 검증
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == null)
                Debug.LogError($"[JamipGlobalFearLibrary] 기본 세트({FolderPath})의 {i}번 공포가 비어 있습니다. " +
                               "Resources/Fears 경로에 FearData 자산이 모두 존재하는지 확인하세요.");
        }

        return arr;
    }
}