using UnityEngine;

public class BaseMovement : MonoBehaviour
{
    public Transform[] characters; // 0: 플1, 1: 플2, 2: 플3
    public int activeIndex = 0;

    public int gridWidth = 12;
    public int gridHeight = 7;

    public Vector2Int currentGridPosition = new Vector2Int(0, 0);
    public FearSelectionManager fearUI;

    void Start()
    {
        SetActiveCharacter(0);
        currentGridPosition = new Vector2Int(0, 0);
        MoveToPosition(currentGridPosition);

        fearUI.EnterBattle();
    }

    void Update()
    {
        HandleMovementInput();
        HandleSwitchInput();
    }

    void HandleMovementInput()
    {
        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            Vector2Int nextPos = currentGridPosition + direction;

            if (IsWithinBounds(nextPos))
            {
                currentGridPosition = nextPos;
                MoveToPosition(currentGridPosition);
            }
        }
    }

    void HandleSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveCharacter(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveCharacter(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetActiveCharacter(2);
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int nextIndex = (activeIndex + 1) % characters.Length;
            SetActiveCharacter(nextIndex);
        }
    }

    void SetActiveCharacter(int index)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].gameObject.SetActive(i == index);
        }
        activeIndex = index;

        // 새 캐릭터 스탯 초기화
        PlayerStatus newPlayer = characters[activeIndex].GetComponent<PlayerStatus>();
        newPlayer.InitializeFromData();
        // 체력 UI 동기화
        newPlayer.playerHealth.InitializeHealth(false);

        MoveToPosition(currentGridPosition);
    }

    public void MoveToPosition(Vector2Int gridPos)
    {
        if (!IsWithinBounds(gridPos))
        {
            Debug.LogWarning($"[MoveToPosition] Out of bounds: {gridPos}");
            return;
        }

        Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
        characters[activeIndex].position = worldPos;
    }

    public bool IsWithinBounds(Vector2Int pos)
    {
        int minX = -6;
        int maxX = 5;
        int minY = -2;
        int maxY = 4;

        return pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY;
    }

    // 셀 좌표 → 그리드 좌표 변환
    Vector2Int CellToGrid(Vector3Int cell)
    {
        // 예시: 타일맵의 (0,0)이 게임 그리드의 (-6,-2)라면
        return new Vector2Int(cell.x, cell.y);
        // 필요시 오프셋 적용: new Vector2Int(cell.x + offsetX, cell.y + offsetY);
    }
}
