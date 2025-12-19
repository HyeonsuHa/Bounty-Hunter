using UnityEngine;

public class TooltipSpawner : MonoBehaviour
{
    public static TooltipSpawner Instance { get; private set; }

    [SerializeField] private TooltipPanel tooltipPrefab;
    [SerializeField] private RectTransform spawnParent; // 보통 Canvas 또는 Panel

    TooltipPanel _current;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (_current) _current.FollowMouse();
    }

    public void Show(string text)
    {
        if (!tooltipPrefab || !spawnParent) return;

        // 이미 떠있으면 재사용(또는 텍스트만 갱신)
        if (_current == null)
            _current = Instantiate(tooltipPrefab, spawnParent);

        _current.SetText(text);
        _current.FollowMouse();
    }

    public void Hide()
    {
        if (_current)
        {
            Destroy(_current.gameObject);
            _current = null;
        }
    }
}
