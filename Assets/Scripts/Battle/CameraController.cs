using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    void Awake()
    {
        Instance = this;
    }

    public void ZoomToBoss()
    {
        // 카메라 줌인 로직 구현
    }

    public void ZoomOutToBattle()
    {
        // 카메라 줌아웃 로직 구현
    }
}