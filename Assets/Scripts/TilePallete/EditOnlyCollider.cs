using UnityEngine;

public class EditOnlyCollider : MonoBehaviour
{
    [SerializeField] private Collider col;

    private void Awake()
    {
        if (!col) col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!col) return;

        bool isEdit = (ModeManager.Instance != null && ModeManager.Instance.CurrentMode == GameMode.Edit);
        col.enabled = isEdit;
    }
}
