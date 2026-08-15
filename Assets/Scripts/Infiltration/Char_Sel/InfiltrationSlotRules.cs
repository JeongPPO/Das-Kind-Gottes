using UnityEngine;

namespace Infiltration
{
    public static class InfiltrationSlotRules
    {
        public static void ApplyCharacterToLoadout(InfiltrationCharacterSO ch, InfiltrationLoadoutSO lo)
        {
            if (ch == null || lo == null) return;

            switch (ch.role)
            {
                case RoleType.Attacker:
                    lo.attacker = ch;
                    lo.A_Tap = ch.roleSlotPrimary;
                    lo.A_Hold = ch.roleSlotSecondary;
                    break;
                case RoleType.Supporter:
                    lo.supporter = ch;
                    lo.Shift = ch.roleSlotPrimary;
                    lo.S_Tap = ch.roleSlotSecondary;
                    break;
                case RoleType.Thief:
                    lo.thief = ch;
                    lo.C_Hold = ch.roleSlotPrimary;
                    lo.D_Tap = ch.roleSlotSecondary;
                    break;
                case RoleType.Healer:
                    lo.healer = ch;
                    lo.Space_Tap = ch.roleSlotPrimary;
                    lo.Space_Hold = ch.roleSlotSecondary;
                    break;
            }
        }
    }
}