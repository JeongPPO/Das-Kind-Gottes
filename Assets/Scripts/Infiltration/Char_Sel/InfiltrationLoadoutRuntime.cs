using UnityEngine;

public class InfiltrationLoadoutRuntime : MonoBehaviour
{
    public static InfiltrationLoadoutRuntime Instance { get; private set; }

    [Tooltip("전투 씬에서 사용할 최종 로드아웃")]
    public InfiltrationLoadoutSO currentLoadout;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLoadout(InfiltrationLoadoutSO so)
    {
        currentLoadout = so;
    }
}