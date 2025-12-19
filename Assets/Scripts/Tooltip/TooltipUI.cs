using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // Input System 쓰면 필요

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [SerializeField] private RectTransform root;   // TooltipRoot
    [SerializeField] private TMP_Text text;        // TooltipText
    [SerializeField] private Vector2 offset = new Vector2(16f, -16f);
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 padding = new Vector2(8f, 8f);

    RectTransform canvasRect;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        canvasRect = canvas.GetComponent<RectTransform>();
        Hide();
    }

    void Update()
    {
        if (root.gameObject.activeSelf)
            FollowMouse();
    }

    public void Show(string message)
    {
        text.text = message;
        root.gameObject.SetActive(true);
        FollowMouse();
    }

    public void Hide()
    {
        root.gameObject.SetActive(false);
    }

    void FollowMouse()
    {
        if (!canvasRect || !root) return;

        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue() + offset;

        // Overlay면 camera는 null
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out var localPoint
        );

        // Clamp
        Vector2 halfCanvas = canvasRect.rect.size * 0.5f;
        Vector2 size = root.rect.size;

        float left = -halfCanvas.x + padding.x + size.x * root.pivot.x;
        float right = halfCanvas.x - padding.x - size.x * (1f - root.pivot.x);
        float bottom = -halfCanvas.y + padding.y + size.y * root.pivot.y;
        float top = halfCanvas.y - padding.y - size.y * (1f - root.pivot.y);

        localPoint.x = Mathf.Clamp(localPoint.x, left, right);
        localPoint.y = Mathf.Clamp(localPoint.y, bottom, top);

        root.anchoredPosition = localPoint;
    }
}
