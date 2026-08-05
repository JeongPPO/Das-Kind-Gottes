using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Yarn.Unity;

public class DialogueUIController : MonoBehaviour
{
    [Header("UI Images")]
    [SerializeField] private Image leftImage;  // 왼쪽 캐릭터 UI
    [SerializeField] private Image rightImage; // 오른쪽 캐릭터 UI
    [SerializeField] private Image centerImage; // 중앙 캐릭터 UI

    [Header("Background CG")]
    [SerializeField] private Image backgroundImage; // 배경용 UI

    [Header("Sprites")]
    [SerializeField] private List<Sprite> portraitSprites;
    [SerializeField] private List<Sprite> backgroundSprites;

    private Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();

    void Awake()
    {
        RegisterSprites(portraitSprites);
        RegisterSprites(backgroundSprites);
    }
    private void RegisterSprites(List<Sprite> sprites)
    {
        foreach (var sprite in sprites)
        {
            if (sprite != null && !spriteDict.ContainsKey(sprite.name.ToLower()))
            {
                spriteDict.Add(sprite.name.ToLower(), sprite);
            }
        }
    }
        // 호출 예시: <<set_face left player_smile>> 또는 <<set_face right guard_angry>>
        // 1. 캐릭터 이미지, 표정 변경 커맨드
        [YarnCommand("set_face")]
    public static void SetFace(string side, string expression)
    {
        // 씬에 존재하는 DialogueUIController 인스턴스를 찾습니다.
        var controller = GameObject.FindFirstObjectByType<DialogueUIController>();

        if (controller != null)
        {
            controller.ExecuteChangeFace(side, expression);
        }
        else
        {
            Debug.LogError("씬에서 DialogueUIController 오브젝트를 찾을 수 없습니다!");
        }
    }

    // 실제 로직 처리
    private void ExecuteChangeFace(string side, string expression)
    {
        string key = expression.ToLower();
        if (!spriteDict.TryGetValue(key, out Sprite targetSprite))
        {
            Debug.LogWarning($"[Dialogue] {expression} 이미지를 찾을 수 없습니다.");
            return;
        }

        if (side.ToLower() == "right")
        {
            rightImage.sprite = targetSprite;
            // 오른쪽은 정방향 (Scale X = 1)
            rightImage.transform.localScale = new Vector3(1, 1, 1);
            rightImage.gameObject.SetActive(true);
        }
        else if (side.ToLower() == "left")
        {
            leftImage.sprite = targetSprite;
            // 왼쪽은 좌우 반전 (Scale X = -1)
            leftImage.transform.localScale = new Vector3(-1, 1, 1);
            leftImage.gameObject.SetActive(true);
        }

        else if (side.ToLower() == "center")
        {
            centerImage.sprite = targetSprite;
            centerImage.transform.localScale = new Vector3(1, 1, 1);
            centerImage.gameObject.SetActive(true);
        }

        SetHighlight(side.ToLower());
    }

    private void SetHighlight(string speakingSide)
    {
        leftImage.color = (speakingSide == "left") ? Color.white : Color.gray;
        rightImage.color = (speakingSide == "right") ? Color.white : Color.gray;
    }

    // 2. 캐릭터 이미지 숨기기 커맨드
    [YarnCommand("hide_face")]
    public static void HideFaceStatic(string side)
    {
        var controller = GameObject.FindFirstObjectByType<DialogueUIController>();

        if (controller != null)
        {
            controller.ExecuteHideFace(side);
        }
    }

    // 실제 숨기기 로직
    private void ExecuteHideFace(string side)
    {
        string s = side.ToLower();
        if (s == "left" || s == "all") leftImage.gameObject.SetActive(false);
        if (s == "right" || s == "all") rightImage.gameObject.SetActive(false);
        if (s == "center" || s == "all") centerImage.gameObject.SetActive(false);
    }

    // 3. 배경 설정 커맨드: <<set_bg 이미지이름>>
    [YarnCommand("set_bg")]
    public static void SetBackground(string imageName)
    {
        var controller = GameObject.FindFirstObjectByType<DialogueUIController>();
        if (controller != null)
        {
            controller.ExecuteSetBackground(imageName);
        }
    }

    private void ExecuteSetBackground(string imageName)
    {
        if (imageName.ToLower() == "none")
        {
            backgroundImage.gameObject.SetActive(false);
            return;
        }

        if (spriteDict.TryGetValue(imageName.ToLower(), out Sprite targetSprite))
        {
            backgroundImage.sprite = targetSprite;
            backgroundImage.gameObject.SetActive(true);
        }
    }

    [YarnCommand("hide_bg")]
    public static void HideBackground()
    {
        var controller = GameObject.FindFirstObjectByType<DialogueUIController>();

        if (controller != null)
        {
            controller.ExecuteHideBackground();
        }
    }

    private void ExecuteHideBackground()
    {
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }
    }
}