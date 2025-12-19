using TMPro;
using UnityEngine;

public class TooltipPanel : MonoBehaviour
{
    [SerializeField] private RectTransform root; // 자기 자신(없으면 GetComponent로 대체 가능)
    [SerializeField] private TMP_Text text;

    public Vector2 offset = new Vector2(16f, -16f);

    RectTransform _rt;

    void Awake()
    {
        _rt = root ? root : GetComponent<RectTransform>();
    }

    public void SetText(string msg)
    {
        if (text) text.text = msg;
    }

    public void FollowMouse()
    {
        Vector2 pos = (Vector2)Input.mousePosition + offset;

        // 화면 밖 방지 (Overlay 기준)
        Vector2 size = _rt.sizeDelta;
        float maxX = Screen.width - size.x;
        float minY = size.y;

        pos.x = Mathf.Clamp(pos.x, 0f, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, Screen.height);

        _rt.position = pos;
    }
}
