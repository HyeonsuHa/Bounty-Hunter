using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea] public string description;
    public float hoverDelay = 0.6f;

    Coroutine _co;
    bool _hover;

    public void SetText(string text) => description = text;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hover = true;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CoShow());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hover = false;
        if (_co != null) StopCoroutine(_co);
        _co = null;

        if (TooltipUI.Instance) TooltipUI.Instance.Hide();
    }

    IEnumerator CoShow()
    {
        yield return new WaitForSeconds(hoverDelay);

        if (_hover && TooltipUI.Instance && !string.IsNullOrWhiteSpace(description))
            TooltipUI.Instance.Show(description);
    }

    // 버튼이 Destroy될 때 툴팁이 남는 상황 방지
    void OnDisable()
    {
        if (_hover && TooltipUI.Instance) TooltipUI.Instance.Hide();
        _hover = false;
        if (_co != null) StopCoroutine(_co);
        _co = null;
    }
}
