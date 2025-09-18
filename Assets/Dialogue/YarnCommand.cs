using UnityEngine;
using Yarn.Unity;
using static FearSelectionManager;

public class YarnCommands : MonoBehaviour
{
    [YarnCommand("zoom_in")]
    public void ZoomIn()
    {
        CameraController.Instance.ZoomToBoss();
    }

    [YarnCommand("zoom_out")]
    public void ZoomOut()
    {
        CameraController.Instance.ZoomOutToBattle();
    }

    [YarnCommand("fear_choice")]
    public void ShowFearChoice()
    {
        FearSelectionManager.Instance.EnterBattle(); // 패널 열기 및 선택 대기
    }


    [YarnCommand("resume_battle")]
    public void ResumeBattle()
    {
        BattleManager.Instance.ResumeBattle();
    }

}