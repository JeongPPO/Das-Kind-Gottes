using UnityEngine;

namespace Infiltration
{
    public static class InfiltrationSlotRules
    {
        // 직군→로드아웃 매핑 규칙
        public static void ApplyCharacterToLoadout(InfiltrationCharacterSO ch, InfiltrationLoadoutSO lo)
        {
            if (ch == null || lo == null) return;

            switch (ch.role)
            {
                case RoleType.Attacker:
                    lo.A_Tap = ch.roleSlotPrimary;
                    lo.A_Hold = ch.roleSlotSecondary;
                    break;

                case RoleType.Supporter:
                    lo.Shift = ch.roleSlotPrimary;
                    lo.S_Tap = ch.roleSlotSecondary;
                    break;

                case RoleType.Thief:
                    lo.C_Hold = ch.roleSlotPrimary;
                    lo.D_Tap = ch.roleSlotSecondary;
                    break;

                case RoleType.Healer:
                    lo.Space_Tap = ch.roleSlotPrimary;
                    lo.Space_Hold = ch.roleSlotSecondary;
                    break;
            }
        }
    }
}