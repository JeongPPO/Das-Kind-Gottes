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
        MoveToPosition(currentGridPosition);
    }

    void MoveToPosition(Vector2Int gridPos)
    {
        if (!IsWithinBounds(gridPos))
        {
            Debug.LogWarning($"[MoveToPosition] Out of bounds: {gridPos}");
            return;
        }

        Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
        characters[activeIndex].position = worldPos;
    }

    bool IsWithinBounds(Vector2Int pos)
    {
        int minX = -6;
        int maxX = 5;
        int minY = -2;
        int maxY = 4;

        return pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY;
    }
}
