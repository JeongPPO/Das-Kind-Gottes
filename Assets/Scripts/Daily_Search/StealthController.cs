using UnityEngine;
using Yarn.Unity;

public class StealthController : MonoBehaviour
{
    public bool IsStealthed { get; private set; }

    public void EnableStealth(bool active)
    {
        IsStealthed = active;
        if (active)
            Debug.Log("은신 시작: 발각 확률 20%");
        else
            Debug.Log("은신 해제");
    }
    // TO DO: 은신 발각 확률 감소 등의 효과 이후 구현 필요!!!!
}