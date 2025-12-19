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

    private CanvasGroup _cg;

    private void Awake()
    {
        if (!tilePlacer) tilePlacer = FindFirstObjectByType<TilePlacer3D>();
        if (!modeManager) modeManager = ModeManager.Instance;

        if (!uiRoot)
        {
            if (contentRoot && contentRoot.parent)
                uiRoot = contentRoot.parent.gameObject;
            else
                uiRoot = gameObject; // 최후의 fallback
        }

        if (uiRoot == gameObject)
        {
            Debug.LogWarning("[TilePaletteUI] uiRoot가 TilePaletteUI가 붙은 오브젝트와 같습니다. " +
                             "SetActive로 끄면 스크립트가 비활성화되어 다시 Edit로 돌아와도 UI가 안 뜰 수 있어 CanvasGroup으로 숨깁니다.");
        }

        _cg = uiRoot.GetComponent<CanvasGroup>();
        if (_cg == null) _cg = uiRoot.AddComponent<CanvasGroup>();

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

        SetUIVisible(isEdit);

        if (!isEdit) return;

        if (_buttons.Count == 0 && tiles.Count > 0)
            BuildButtons();

        if (_selectedIndex < 0 && tiles.Count > 0)
            _selectedIndex = 0;

        if (_selectedIndex >= 0 && _selectedIndex < tiles.Count && tiles[_selectedIndex] != null)
            if (tilePlacer) tilePlacer.SetSelectedTile(tiles[_selectedIndex]);

        RefreshHighlight();
    }

    private void SetUIVisible(bool visible)
    {
        if (_cg == null) return;

        _cg.alpha = visible ? 1f : 0f;
        _cg.interactable = visible;
        _cg.blocksRaycasts = visible;

        // Edit 모드가 아니면 툴팁은 반드시 내려가도록
        if (!visible && TooltipUI.Instance)
            TooltipUI.Instance.Hide();
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

            // ===== View로 UI 세팅 (아이콘/라벨 참조 안정화) =====
            var view = btn.GetComponent<TileButtonView>();
            if (view != null)
            {
                // 네 TileButtonView가 def/icon을 다루는 방식에 맞게 조정
                view.Set(def, idx + 1);
                // 만약 Set이 없다면 아래처럼 직접:
                // view.Def = def;
                // if (view.IconImage) { view.IconImage.sprite = def.icon; view.IconImage.enabled = def.icon != null; }
            }
            else
            {
                // fallback(뷰 없을 때만)
                var label = btn.GetComponentInChildren<TMP_Text>(true);
                if (label) label.text = $"{idx + 1}";

                var icon = btn.transform.Find("Icon")?.GetComponent<Image>();
                if (icon)
                {
                    icon.sprite = def.icon;
                    icon.enabled = def.icon != null;
                }
            }

            // ===== 클릭 이벤트 =====
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectIndex(idx));

            // ===== TooltipTrigger =====
            // 버튼 루트에 붙여도 되지만, 가장 확실한 건 "Target Graphic"이 붙은 오브젝트(보통 btn의 Image)에 붙이는 것
            var triggerHost = btn.targetGraphic != null ? btn.targetGraphic.gameObject : btn.gameObject;

            var tt = triggerHost.GetComponent<TooltipTrigger>();
            if (tt == null) tt = triggerHost.AddComponent<TooltipTrigger>();

            string desc = def.description;

            if (string.IsNullOrWhiteSpace(desc))
            {
                tt.enabled = false;
            }
            else
            {
                tt.enabled = true;
                tt.SetText(desc);
                tt.hoverDelay = 0.6f;
            }

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
