using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TilePaletteUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TilePlacer3D tilePlacer;   // 선택 반영 대상
    [SerializeField] private ModeManager modeManager;   // Edit 모드 체크

    [Header("Tiles (1,2,3... order)")]
    [SerializeField] private List<TileDefinition> tiles = new();

    [Header("UI")]
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform contentRoot;

    [Header("Highlight")]
    [SerializeField] private Color normalColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.25f, 0.55f, 1f, 1f);

    private readonly List<Button> _buttons = new();
    private int _selectedIndex = -1;

    private void Awake()
    {
        if (!tilePlacer) tilePlacer = FindFirstObjectByType<TilePlacer3D>();
        if (!modeManager) modeManager = ModeManager.Instance;

        BuildButtons();
        SelectIndex(0); // 시작은 1번 타일
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
        // Edit 모드에서만 UI 표시
        if (contentRoot) contentRoot.gameObject.SetActive(isEdit);
    }

    private void Update()
    {
        // 숫자키 1/2/3 선택 (New Input System)
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            SelectIndex(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            SelectIndex(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            SelectIndex(2);
    }

    private void BuildButtons()
    {
        // 기존 버튼 제거
        foreach (var b in _buttons)
            if (b) Destroy(b.gameObject);
        _buttons.Clear();

        if (!buttonPrefab || !contentRoot) return;

        for (int i = 0; i < tiles.Count; i++)
        {
            int idx = i;
            var def = tiles[i];

            var btn = Instantiate(buttonPrefab, contentRoot);
            _buttons.Add(btn);

            // 라벨: "1 LavaTile" 형태
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = $"{idx + 1}. {def.name}";

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectIndex(idx));

            SetButtonColor(btn, normalColor);
        }
    }

    private void SelectIndex(int idx)
    {
        if (idx < 0 || idx >= tiles.Count) return;
        if (tiles[idx] == null) return;

        _selectedIndex = idx;

        // 타일 설치 시스템에 반영
        if (tilePlacer) tilePlacer.SetSelectedTile(tiles[idx]);

        // 하이라이트 갱신
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
