using UnityEngine;

public class CameraPivotFollower : MonoBehaviour
{
    [SerializeField] private Transform target;   // Player 넣기
    [SerializeField] private Vector3 offset = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        // 위치만 따라가고, 회전은 카메라가 직접 관리
        transform.position = target.position + offset;
    }
}
