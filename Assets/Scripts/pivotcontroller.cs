using UnityEngine;

public class CameraPivotController : MonoBehaviour
{
    [Header("플레이어")]
    public Transform target;

    [Header("플레이어 기준 오프셋")]
    public Vector3 offset = new Vector3(0f, 3.5f, 0f);

    [Header("고정 시점 회전 (월드 기준)")]
    public Vector3 worldEuler = new Vector3(30f, 0f, 0f); // 예: 살짝 위에서 내려보는 각도

    private void LateUpdate()
    {
        if (target == null) return;

        // 1) 위치는 플레이어 + 오프셋
        transform.position = target.position + offset;

        // 2) 회전은 부모랑 상관없이, 월드 기준으로 내가 지정한 각도 유지
        transform.rotation = Quaternion.Euler(worldEuler);
    }
}
