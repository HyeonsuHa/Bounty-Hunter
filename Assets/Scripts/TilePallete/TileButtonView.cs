using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileButtonView : MonoBehaviour
{
    [SerializeField] private TileDefinition def;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    private void Awake()
    {
        if (def && iconImage) iconImage.sprite = def.icon;
    }
    public void Set(TileDefinition def, int number)
    {
        if (iconImage)
        {
            iconImage.sprite = def.icon;
            iconImage.enabled = def.icon != null;
        }

        if (labelText)
            labelText.text = number.ToString();
    }
    public TileDefinition Def => def;
}
