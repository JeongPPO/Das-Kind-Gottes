using System.Collections;
using UnityEngine;
using Yarn;
using Yarn.Unity;
using static FearData;
using static FearSelectionManager;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public Animator playerAnimator;
    private PlayerStatus player;
    private PlayerData playerData;

    [Header("Yarn")]
    public DialogueRunner dialogueRunner;
    public string battleStartNode = "BattleStart_Boss1";

    [Header("전투 상태")]
    public bool isBattleStarted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        player = FindFirstObjectByType<PlayerStatus>();
    }

    void Start()
    {
        StartCoroutine(StartIntroSequence());
        StartBattle();
    }
    IEnumerator StartIntroSequence()
    {
        yield return new WaitForSeconds(1f); // 씬 로딩 후 약간의 딜레이
        playerAnimator.SetTrigger("IntroStart");
        yield return new WaitForSeconds(2.5f); // 애니메이션 시간 대기
    }

    public void StartBattle()
    {
        if (dialogueRunner != null)
        {
            isBattleStarted = true;
            dialogueRunner.StartDialogue(battleStartNode);
        }
        else
        {
            Debug.LogError("DialogueRunner가 연결되지 않았습니다!");
        }
    }

    // 공포 선택 결과 받아서 효과 적용
    public void OnFearSelected(FearData selectedFearData)
    {
        Debug.Log("선택된 공포: " + selectedFearData.fearType);

        EnemyData currentEnemy = EnemyManager.Instance.CurrentEnemy;
        if (currentEnemy == null)
        {
            Debug.LogWarning("현재 전투 중인 적 정보가 없습니다.");
            return;
        }

        bool isCorrect = selectedFearData.fearType == currentEnemy.trueFear;

        if (isCorrect)
        {
            ApplyBuff(selectedFearData.fearType);
        }
        else
        {
            ApplyDebuff(selectedFearData.fearType);
        }

        ResumeBattle();
    }

    public void ApplyBuff(FearData.FearType fear)
    {
        Debug.Log($"[버프 적용] {fear} -> 공격력 10% 증가");
        // 여기에 버프 로직 구현
        player.attackPower *= 1.1f;
    }

    public void ApplyDebuff(FearData.FearType fear)
    {
        Debug.Log($"[디버프 적용] {fear}");

        switch (fear)
        {
            case FearData.FearType.Deficiency:
                StartCoroutine(Debuff_Deficiency(10f, 0.05f));
                break;

            case FearData.FearType.Death:
                StartCoroutine(Debuff_Death(10f, 0.8f));
                break;

            case FearData.FearType.Isolation:
                StartCoroutine(Debuff_Isolation(15f, 0.1f));
                break;

            case FearData.FearType.Humiliation:
                Debuff_Humiliation();
                break;

            case FearData.FearType.Failure:
                StartCoroutine(Debuff_Failure(20f, 0.5f, 0.5f));
                break;
        }
    }

    #region 각 디버프 구현
    private IEnumerator Debuff_Deficiency(float duration, float hpLossPercent)
    {
        player.canHeal = false;
        float tick = 1f;
        float interval = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            player.currentHP -= PlayerData.maxHP * hpLossPercent;
            Debug.Log($"결핍 피해: {player.maxHP * hpLossPercent} HP 감소");
            elapsed += interval;
            elapsed += tick;
            yield return new WaitForSecondsRealtime(interval);
        }

        player.canHeal = true;
    }

    private IEnumerator Debuff_Death(float duration, float lethalDamagePercent)
    {
        player.isLethalOnHit = true;
        player.lethalDamagePercent = lethalDamagePercent;
        yield return new WaitForSecondsRealtime(duration);
        player.isLethalOnHit = false;
    } // 보스 기믹 아이디어: 어떤 보스는 0.8보다 더 많은 데미지를 주는데 죽음의 공포 활용해서 0.8 정도로 낮추어서 피해 받을 수 있음.

    private IEnumerator Debuff_Isolation(float duration, float speedReduction)
    {
        float originalSpeed = player.moveSpeed;
        // 캐릭터 교체 금지
        player.canSwitchCharacter = false;
        // 속도를 일정 비율로 감소
        player.moveSpeed = originalSpeed * speedMultiplier;
        yield return new WaitForSecondsRealtime(duration);
        // 원래 속도로 복구
        player.moveSpeed = originalSpeed;
        // 캐릭터 교체 가능
        player.canSwitchCharacter = true;
    }

    private void Debuff_Humiliation()
    {
        player.extraHP = 0;
        player.DisableEquipmentEffects();
    }

    private IEnumerator Debuff_Failure(float duration, float cooldownIncreaseRate, float gaugeRate)
    {
            float originalCooldownMultiplier = player.skillCooldownMultiplier;
            float originalGaugeRate = player.fearGaugeChargeRate;

            player.skillCooldownMultiplier = originalCooldownMultiplier + cooldownIncreaseRate;
            player.fearGaugeChargeRate = originalGaugeRate * gaugeRate;

            yield return new WaitForSecondsRealtime(duration);
            player.skillCooldownMultiplier = originalCooldownMultiplier;
            player.fearGaugeChargeRate = originalGaugeRate;
        }
    #endregion

    public void ResumeBattle()
    {
        Debug.Log("전투 재개!");
        Time.timeScale = 1f;
        // TODO: 플레이어 컨트롤 재개, 공격 루프 활성화 등
    }

    // 몬스터 모드 진입 시 외부에서 호출
    public void TriggerMonsterMode()
    {
        Debug.Log("몬스터 모드 진입! 특수 연출 실행");
        // TODO: Yarn 대사 재생, 애니메이션 트리거 등 처리
    }
}