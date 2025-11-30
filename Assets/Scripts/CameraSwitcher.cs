using Unity.Cinemachine;
using UnityEngine;

public class CameraViewSwitcher : MonoBehaviour
{
    [Header("Virtual Cameras")]
    public CinemachineCamera normalCam;   // 기본 뷰
    public CinemachineCamera topDownCam;  // 탑뷰

    [Header("Priorities")]
    public int normalPriority = 10;
    public int topDownPriority = 11;

    private bool _isTopDown = false;

    private void Start()
    {
        // 시작은 기본 카메라 사용
        SetTopDown(false);
    }

    private void Update()
    {
        // 예시: Tab 키로 카메라 토글
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _isTopDown = !_isTopDown;
            SetTopDown(_isTopDown);
        }
    }

    private void SetTopDown(bool useTopDown)
    {
        if (useTopDown)
        {
            topDownCam.Priority = topDownPriority;
            normalCam.Priority = normalPriority;
        }
        else
        {
            topDownCam.Priority = normalPriority;
            normalCam.Priority = topDownPriority;
        }
    }
}
