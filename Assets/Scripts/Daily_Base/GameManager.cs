using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public SaveData loadedData;
    [HideInInspector] public float currentPlayTime = 0f;

    [Header("Characters (Assign in Inspector)")]
    public List<CharacterDataSO> allCharacters = new List<CharacterDataSO>();

    [Header("Battle Session")]
    public BattleSession currentBattleSession;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<CharacterDataSO> GetEncounteredCharacters()
    {
        // 지금은 전체 캐릭터 반환, 나중에 특정 조건으로 필터링 가능
        return allCharacters;
    }

    private void Update()
    {
        // TitleScene 제외하고 플레이타임 누적
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "TitleScene")
        {
            currentPlayTime += Time.deltaTime;
        }
    }

    public void ResetPlayTime() => currentPlayTime = 0f;
    public float GetPlayTime() => currentPlayTime;

    public void SetLoadedData(SaveData data)
    {
        loadedData = data;
        currentPlayTime = data != null ? data.playTime : 0f;
    }

    public void StartBattle(EnemyData enemyData, string returnScene, Vector3 returnPosition)
    {
        currentBattleSession = new BattleSession(enemyData, returnScene, returnPosition);
        SceneManager.LoadScene("BattleScene");
    }

    public void ClearBattleSession()
    {
        currentBattleSession = null;
    }

    [System.Serializable]
    public class BattleSession
    {
        public EnemyData enemy;
        public string returnScene;
        public Vector3 returnPosition;

        public BattleSession(EnemyData enemy, string returnScene, Vector3 returnPosition)
        {
            this.enemy = enemy;
            this.returnScene = returnScene;
            this.returnPosition = returnPosition;
        }
    }

    public void Fail(string reason)
    {
        Debug.Log($"❌ 게임 실패! 이유: {reason}");

        // TODO: 실패 처리 로직
        // 예: 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // 또는 실패 UI 띄우기
        // UIManager.Instance.ShowFailPanel(reason);
    }

}