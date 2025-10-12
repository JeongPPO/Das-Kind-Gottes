using UnityEngine;
using static FearSelectionManager;

public class StatusEffectManager : MonoBehaviour
{
    public PlayerStatus playerStatus;

    public void ApplyEffectBasedOnFear(FearData.FearType fear)
    {
        switch (fear)
        {
            case FearData.FearType.Deficiency:
                playerStatus.attackPower -= 0.2f;
                break;
            case FearData.FearType.Death:
                playerStatus.attackPower -= 25f;
                break;
            case FearData.FearType.Isolation:
                playerStatus.moveSpeed -= 0.5f;
                break;
            case FearData.FearType.Humiliation:
                playerStatus.defense -= 0.5f;
                break;
            case FearData.FearType.Failure:
                playerStatus.critChance += 0.15f;
                break;
        }
    }
}
