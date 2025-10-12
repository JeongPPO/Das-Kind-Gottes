using System.Collections;
using UnityEngine;
using Yarn.Unity; // YarnSpinner 연동

[RequireComponent(typeof(InvestigationHighlighter))]
[RequireComponent(typeof(StealthController))]
public class DailyPlayerController : MonoBehaviour
{
    [Header("Characters")]
    public Transform[] characters; // 0: 이동/설득, 1: 조사, 2: 습격, 3: 은신
    private int activeIndex = 0;

    [Header("Grid")]
    public int gridWidth = 12;
    public int gridHeight = 7;
    public Vector2Int currentGridPosition = Vector2Int.zero;
    public float baseMoveCooldown = 0.2f;
    private float moveCooldown;
    private float moveTimer = 0f;

    [Header("Managers")]
    public AssaultManager assaultManager;
    public DialogueRunner dialogueRunner;

    private InvestigationHighlighter investigationHighlighter;
    private StealthController stealthController;

    private bool isStealthed = false;


    void Awake()
    {
        investigationHighlighter = GetComponent<InvestigationHighlighter>();
        stealthController = GetComponent<StealthController>();
    }

    void Start()
    {
        moveCooldown = baseMoveCooldown;
        SetActiveCharacter(0);
        MoveToGrid(currentGridPosition);
    }

    void Update()
    {
        HandleMovement();
        HandleAbilities();
        HandlePersuasion();
    }

    // ========== 이동 ==========
    void HandleMovement()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer > 0f) return;

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
        {
            Vector2Int nextPos = currentGridPosition + dir;
            if (IsWithinBounds(nextPos))
            {
                currentGridPosition = nextPos;
                MoveToGrid(currentGridPosition);
                moveTimer = moveCooldown;
            }
        }
    }

    // ========== 능력 ==========
    void HandleAbilities()
    {
        // 은신 (W, Space 안 눌림)
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.Space))
        {
            SetActiveCharacter(3);

            if (!isStealthed)
            {
                isStealthed = true;
                moveCooldown = baseMoveCooldown / 0.8f; // 이동 ↓
                stealthController.EnableStealth(true);  // 발각 확률 20%
            }

            // 은신 중에는 조사 꺼짐
            InvestigationManager.SetHighlightAll(false);
        }
        // 습격 (W + Space)
        else if (Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.Space))
        {
            SetActiveCharacter(2);
            InvestigationManager.SetHighlightAll(false);
            // 실제 StartAssault() 실행은 외부에서 호출
        }
        // 조사 (Q)
        else if (Input.GetKey(KeyCode.Q))
        {
            SetActiveCharacter(1);
            InvestigationManager.SetHighlightAll(true); // 하이라이트 켜기
        }
        // 기본 상태
        else
        {
            SetActiveCharacter(0);

            if (isStealthed)
            {
                isStealthed = false;
                moveCooldown = baseMoveCooldown;
                stealthController.EnableStealth(false);
            }

            InvestigationManager.SetHighlightAll(false);
        }
    }

    // ========== 설득 ==========
    void HandlePersuasion()
    {
        if (activeIndex == 0 && Input.GetKeyDown(KeyCode.Return))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (var hit in hits)
            {
                var target = hit.GetComponent<PersuasionTarget>();
                if (target != null && target.data != null)
                {
                    dialogueRunner.StartDialogue(target.data.yarnNodeName);
                    break;
                }
            }
        }
    }

    // ========== 공통 ==========
    void SetActiveCharacter(int index)
    {
        for (int i = 0; i < characters.Length; i++)
            characters[i].gameObject.SetActive(i == index);

        activeIndex = index;
        MoveToGrid(currentGridPosition);
    }

    void MoveToGrid(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);
        characters[activeIndex].position = worldPos;
    }

    bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    // 습격 시작
    public void StartAssault(AssaultPatternSO pattern)
    {
        StartCoroutine(assaultManager.RunAssault(pattern));
    }
}
