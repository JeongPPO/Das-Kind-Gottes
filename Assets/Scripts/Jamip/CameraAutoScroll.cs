using UnityEngine;

public class CameraAutoScroll : MonoBehaviour
{
    public enum ScrollDirection { Right, Left, Up, Down }

    [Header("Scroll")]
    public ScrollDirection scrollDir = ScrollDirection.Right;
    public float scrollSpeed = 2f;

    [Header("Optional Reference (미사용 가능)")]
    public UnityEngine.Transform playerTransform;

    [Header("Legacy (JamipController에서 실패 판정 처리 → 현재 미사용)")]
    public float failMargin = 2f;

    [Header("Z 고정 옵션")]
    public bool lockZ = true;
    public float fixedZ = -10f;

    void Update()
    {
        Scroll();

        if (lockZ)
        {
            Vector3 p = transform.position;
            p.z = fixedZ;
            transform.position = p;
        }
    }

    void Scroll()
    {
        float dt = Time.deltaTime;
        Vector3 pos = transform.position;
        switch (scrollDir)
        {
            case ScrollDirection.Right: pos.x += scrollSpeed * dt; break;
            case ScrollDirection.Left:  pos.x -= scrollSpeed * dt; break;
            case ScrollDirection.Up:    pos.y += scrollSpeed * dt; break;
            case ScrollDirection.Down:  pos.y -= scrollSpeed * dt; break;
        }
        transform.position = pos;
    }

    // 외부(JamipController 등)에서 현재 카메라 가시 월드 영역 필요할 때 호출
    public Rect GetViewWorldRect(Camera cam = null)
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return new Rect();

        float h = 2f * cam.orthographicSize;
        float w = h * cam.aspect;
        Vector3 c = cam.transform.position;
        return new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h);
    }
}
