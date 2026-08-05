using UnityEngine;
using System.Collections;
using Infiltration;

public class InfiltrationEnemyMover : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite frontSprite;
    public Sprite backSprite;
    public Sprite sideSprite;

    private InfiltrationGridManager _grid;
    private SpriteRenderer _sr;
    public bool IsMoving { get; private set; }

    void Awake()
    {
        _grid = InfiltrationGridManager.Instance;
        _sr = GetComponentInChildren<SpriteRenderer>();
    }

    public IEnumerator MoveToRoutine(Vector2Int targetGrid, float speed)
    {
        IsMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = _grid.GridToWorld(targetGrid);
        Vector3 dir = (endPos - startPos).normalized;

        // 1. 시야 방향 회전 (부모)
        if (dir.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 2. 비주얼 업데이트 (자식 스프라이트)
        UpdateVisuals(dir);

        // 3. 실제 이동
        float duration = Vector3.Distance(startPos, endPos) / speed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        IsMoving = false;
    }

    private void UpdateVisuals(Vector3 dir)
    {
        if (_sr == null) return;
        _sr.transform.rotation = Quaternion.identity; // 이미지는 회전 고정

        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            _sr.sprite = dir.y > 0 ? backSprite : frontSprite;
            _sr.flipX = false;
        }
        else
        {
            _sr.sprite = sideSprite;
            _sr.flipX = dir.x < 0;
        }
    }
}