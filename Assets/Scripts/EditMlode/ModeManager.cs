using UnityEngine;
using UnityEngine.InputSystem;

public class ModeManager : MonoBehaviour
{
    public static ModeManager Instance { get; private set; }

    public GameMode CurrentMode { get; private set; } = GameMode.Edit;
    public event System.Action<GameMode> OnModeChanged;

    private InputSystem_Actions _input;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _input = new InputSystem_Actions();
        ApplyActionMap(CurrentMode);
    }

    private void OnEnable()
    {
        ApplyActionMap(CurrentMode);
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleMode();
            Debug.Log($"[ModeManager] Mode -> {CurrentMode}");
        }
    }

    public void SetMode(GameMode newMode)
    {
        if (CurrentMode == newMode)
            return;

        CurrentMode = newMode;
        ApplyActionMap(CurrentMode);
        OnModeChanged?.Invoke(CurrentMode);
    }

    public void ToggleMode()
    {
        SetMode(CurrentMode == GameMode.Edit ? GameMode.Play : GameMode.Edit);
    }

    private void ApplyActionMap(GameMode mode)
    {
        if (mode == GameMode.Play)
        {
            _input.EditMode.Disable();
            _input.Player.Enable();
        }
        else
        {
            _input.Player.Disable();
            _input.EditMode.Enable();
        }
        Debug.Log($"[InputMap] Player:{_input.Player.enabled} Edit:{_input.EditMode.enabled}");
    }

    public InputSystem_Actions Input => _input;
}
