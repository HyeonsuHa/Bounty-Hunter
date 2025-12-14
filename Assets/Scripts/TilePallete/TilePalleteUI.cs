using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TilePaletteUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TilePlacer3D tilePlacer;
    [SerializeField] private ModeManager modeManager;

    [Header("Tiles (1,2,3... order)")]
    [SerializeField] private List<TileDefinition> tiles = new();

    [Header("UI")]
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject uiRoot;

    [Header("Highlight")]
    [SerializeField] private Color normalColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.25f, 0.55f, 1f, 1f);

    private readonly List<Button> _buttons = new();
    private int _selectedIndex = -1;

    private void Awake()
    {
        if (!tilePlacer) tilePlacer = FindFirstObjectByType<TilePlacer3D>();
        if (!modeManager) modeManager = ModeManager.Instance;
        if (!uiRoot) uiRoot = contentRoot ? contentRoot.gameObject : gameObject;

        BuildButtons();
        if (_selectedIndex < 0 && tiles.Count > 0) SelectIndex(0);
        else RefreshHighlight();
    }

    private void OnEnable()
    {
        if (modeManager != null)
            modeManager.OnModeChanged += OnModeChanged;

        if (modeManager != null)
            OnModeChanged(modeManager.CurrentMode);
    }

    private void OnDisable()
    {
        if (modeManager != null)
            modeManager.OnModeChanged -= OnModeChanged;
    }

    private void OnModeChanged(GameMode mode)
    {
        bool isEdit = (mode == GameMode.Edit);

        if (uiRoot) uiRoot.SetActive(isEdit);

        if (isEdit)
        {
            if (_buttons.Count == 0 && tiles.Count > 0)
                BuildButtons();

            if (_selectedIndex < 0 && tiles.Count > 0)
                _selectedIndex = 0;

            if (_selectedIndex >= 0 && _selectedIndex < tiles.Count && tiles[_selectedIndex] != null)
                if (tilePlacer) tilePlacer.SetSelectedTile(tiles[_selectedIndex]);

            RefreshHighlight();
        }
    }

    private void Update()
    {
        if (modeManager != null && modeManager.CurrentMode != GameMode.Edit) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            SelectIndex(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            SelectIndex(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            SelectIndex(2);
    }

    public void Rebuild()
    {
        BuildButtons();
        if (tiles.Count > 0)
        {
            int idx = Mathf.Clamp(_selectedIndex, 0, tiles.Count - 1);
            if (tiles[idx] == null) idx = 0;
            SelectIndex(idx);
        }
        else
        {
            _selectedIndex = -1;
            RefreshHighlight();
        }
    }

    private void BuildButtons()
    {
        foreach (var b in _buttons)
            if (b) Destroy(b.gameObject);
        _buttons.Clear();

        if (!buttonPrefab || !contentRoot) return;

        for (int i = 0; i < tiles.Count; i++)
        {
            int idx = i;
            var def = tiles[i];
            if (!def) continue;

            var btn = Instantiate(buttonPrefab, contentRoot);
            _buttons.Add(btn);

            var images = btn.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.gameObject.name == "Icon")
                {
                    img.sprite = def.icon;
                    img.enabled = def.icon != null;
                    break;
                }
            }

            // 텍스트 (있으면)
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = $"{idx + 1}";

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectIndex(idx));

            SetButtonColor(btn, normalColor);
        }

        RefreshHighlight();
    }

    private void SelectIndex(int idx)
    {
        if (idx < 0 || idx >= tiles.Count) return;
        if (tiles[idx] == null) return;

        _selectedIndex = idx;

        if (tilePlacer) tilePlacer.SetSelectedTile(tiles[idx]);

        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            var btn = _buttons[i];
            if (!btn) continue;

            SetButtonColor(btn, i == _selectedIndex ? selectedColor : normalColor);
        }
    }

    private static void SetButtonColor(Button btn, Color c)
    {
        var img = btn.GetComponent<Image>();
        if (img) img.color = c;
    }
}
