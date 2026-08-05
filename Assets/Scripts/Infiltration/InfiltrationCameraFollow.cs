using UnityEngine;

[RequireComponent(typeof(Camera))]
public class InfiltrationCameraFollow : MonoBehaviour
{
    public Transform target;
    [Tooltip("가로 17 x 세로 11 타일 시야 기준 (무한 스크롤)")]
    public Vector2 viewTiles = new Vector2(17, 11);
    public float followLerp = 12f;

    private Camera cam;
    private InfiltrationGridManager grid;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        grid = InfiltrationGridManager.Instance;
        if (grid == null)
            Debug.LogWarning("[InfiltrationCameraFollow] GridManager 인스턴스가 없습니다.");

        // 세로 11타일이 카메라 높이에 해당되도록 Orthographic Size 설정
        float ts = (grid != null) ? grid.TileSize : 1f;
        cam.orthographic = true;
        cam.orthographicSize = (viewTiles.y * ts) * 0.5f;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 플레이어의 위치를 향해 카메라의 X, Y 좌표 지정 (Z는 유지)
        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);

        // [수정 핵심] 기존의 ClampCameraToGrid 로직 삭제
        // 이제 맵의 경계에 카메라가 부딪히지 않고, 캐릭터가 가는 곳이라면 어디든 무한히 쫓아갑니다.

        transform.position = Vector3.Lerp(transform.position, goal, Time.deltaTime * followLerp);
    }
}