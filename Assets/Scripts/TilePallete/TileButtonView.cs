using UnityEngine;
using UnityEngine.UI;

public class TileButtonView : MonoBehaviour
{
    [SerializeField] private TileDefinition def;
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        if (def && iconImage) iconImage.sprite = def.icon;
    }

    public TileDefinition Def => def;
}
