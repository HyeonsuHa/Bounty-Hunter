using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinataGoalManager : MonoBehaviour
{
    public static PinataGoalManager Instance { get; private set; }

    [Header("Clear")]
    [SerializeField] private float clearDelay = 2f;
    [SerializeField] private bool requirePlayMode = true;

    private readonly HashSet<PinataTile> _alive = new();
    private bool _clearing;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(PinataTile tile)
    {
        if (!tile) return;
        _alive.Add(tile);
    }

    public void Unregister(PinataTile tile)
    {
        if (!tile) return;
        _alive.Remove(tile);

        // 이미 클리어 진행 중이면 중복 방지
        if (_clearing) return;

        // 플레이 중에만 클리어 판정(원하면 끌 수 있음)
        if (requirePlayMode && ModeManager.Instance != null && ModeManager.Instance.CurrentMode != GameMode.Play)
            return;

        if (_alive.Count == 0)
            StartCoroutine(CoClear());
    }

    private IEnumerator CoClear()
    {
        _clearing = true;
        yield return new WaitForSeconds(clearDelay);

        // 스테이지 클리어 → Edit 모드로
        if (ModeManager.Instance != null)
            ModeManager.Instance.SetMode(GameMode.Edit);

        _clearing = false;
    }
}
