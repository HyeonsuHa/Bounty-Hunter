using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TilePaletteButton : MonoBehaviour
{
    [SerializeField] private Image highlight;
    [SerializeField] private TMP_Text label;

    public void SetSelected(bool selected, int hotkeyNumber)
    {
        if (label) label.text = hotkeyNumber.ToString();
        if (highlight) highlight.enabled = selected; // 또는 색상 변경
    }
}
