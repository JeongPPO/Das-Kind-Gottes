using System;

namespace Infiltration
{
    public enum RoleType
    {
        Attacker,
        Supporter,
        Thief,
        Healer
    }

    [Flags]
    public enum InputSlotMask
    {
        None        = 0,
        A_Tap       = 1 << 0,
        A_Hold      = 1 << 1,
        S_Tap       = 1 << 2,
        Shift       = 1 << 3,
        C_Hold      = 1 << 4,
        D_Tap       = 1 << 5,
        Space_Tap   = 1 << 6,
        Space_Hold  = 1 << 7,
        All         = ~0
    }

    public enum SkillType
    {
        LineAttack,     // 직선 범위 공격 (타일 수, 피해)
        AreaSmash,      // 앞 3x3 등 영역 강타
        Dash,           // 그리드 대시
        Heal,           // 회복
        Parry,          // 패링
        Stealth,        // 은신(홀드)
        Steal,          // 훔치기(인접)
        BuffDebuff      // 버프/디버프(서포터 S)
    }

    public enum EnemyState
    {
        Patrol, // 평상시 순찰
        Chase,  // 플레이어 발견 및 추격
        Search, // 플레이어를 놓쳤을 때 주변 탐색
        Stun    // 스킬에 의해 기절함
    }
}