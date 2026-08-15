using Infiltration;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePortraitUI : MonoBehaviour
{
    [SerializeField] private RoleType roleType;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;

    private InfiltrationCharacterSO currentCharacter;
    private Coroutine emotionResetCoroutine;

    public RoleType RoleType => roleType; // 매니저가 직군으로 슬롯을 찾을 때 사용

    private void Start()
    {
        //초기화 코드
        InitFromRuntime();
    }

    // 런타임 저장소에서 내 직군 캐릭터를 읽어와 세팅
    public void InitFromRuntime()
    {
        var loadout = InfiltrationLoadoutRuntime.Instance?.CurrentLoadout;
        if (loadout == null) return;

        var character = loadout.GetCharacterByRole(roleType);
        Setup(character);
    }

    // 외부(매니저, 전투 시작 로직)에서 직접 캐릭터를 주입
    public void Setup(InfiltrationCharacterSO character)
    {
        currentCharacter = character;
        if (currentCharacter == null) return;

        if (nameText) nameText.text = currentCharacter.displayName;
        SetEmotion(EmotionState.Normal);
    }

    public void SetEmotion(EmotionState state, float duration = 0f)
    {
        if (currentCharacter == null || portraitImage == null) return;

        Sprite targetSprite = currentCharacter.GetSprite(state);
        if (targetSprite != null) portraitImage.sprite = targetSprite;

        if (duration > 0f)
        {
            if (emotionResetCoroutine != null) StopCoroutine(emotionResetCoroutine);
            emotionResetCoroutine = StartCoroutine(ResetEmotionRoutine(duration));
        }
    }

    private IEnumerator ResetEmotionRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetEmotion(EmotionState.Normal);
    }

    // Yarn Spinner
    public void SetEmotionByString(string emotionName, float duration = 0f)
    {
        if (System.Enum.TryParse(emotionName, true, out EmotionState state))
        {
            SetEmotion(state, duration);
        }
        else
        {
            Debug.LogWarning($"[BattleUI] 알 수 없는 표정 상태: {emotionName}");
        }
    }
}