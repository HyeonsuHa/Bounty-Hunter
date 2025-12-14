using UnityEngine;

public class EditOnlyUI : MonoBehaviour
{
    [SerializeField] private GameObject targetRoot; // 비우면 자기 자신

    private void Awake()
    {
        if (!targetRoot) targetRoot = gameObject;
    }

    private void OnEnable()
    {
        if (ModeManager.Instance != null)
        {
            ModeManager.Instance.OnModeChanged += OnModeChanged;
            OnModeChanged(ModeManager.Instance.CurrentMode); // 현재 모드로 즉시 반영
        }
    }

    private void OnDisable()
    {
        if (ModeManager.Instance != null)
            ModeManager.Instance.OnModeChanged -= OnModeChanged;
    }

    private void OnModeChanged(GameMode mode)
    {
        bool isEdit = (mode == GameMode.Edit);
        if (targetRoot) targetRoot.SetActive(isEdit);
    }
}
