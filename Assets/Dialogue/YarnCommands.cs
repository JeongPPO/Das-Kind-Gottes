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

    [YarnCommand("character_sprite_anim")]
    public void ShowCharacterSpriteAnim(string spriteName, string animTrigger)
    {
        var spriteManager = FindFirstObjectByType<CharacterSpriteManager>();
        if (spriteManager != null)
        {
            spriteManager.ShowCharacterSpriteAnim(spriteName, animTrigger);
        }
        else
        {
            Debug.LogWarning("CharacterSpriteManager를 찾을 수 없습니다.");
        }
    }
}