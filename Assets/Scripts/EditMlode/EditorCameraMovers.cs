using UnityEngine;

public class EditorCameraMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private void Update()
    {
        if (ModeManager.Instance.CurrentMode != GameMode.Edit) return;

        Vector2 v = ModeManager.Instance.Input.EditMode.CameraMove.ReadValue<Vector2>();
        if (v != Vector2.zero) Debug.Log($"[EditCamMove] {v}");

        Vector3 dir = new Vector3(v.x, 0f, v.y);
        transform.position += dir * (moveSpeed * Time.deltaTime);
    }
}
