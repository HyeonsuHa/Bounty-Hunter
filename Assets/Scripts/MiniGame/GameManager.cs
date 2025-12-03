using System.Collections.Generic;
using UnityEngine;

public enum GameFlowState
{
    None,
    WaitingToStart,
    PlayingMiniGame,
    AllMiniGamesCleared,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Mini Game Settings")]
    [Tooltip("순서대로 진행할 미니게임 프리팹 리스트")]
    [SerializeField] private List<MiniGameBase> _miniGamePrefabs;

    [Tooltip("미니게임들이 생성될 부모 Transform (없으면 null)")]
    [SerializeField] private Transform _miniGameParent;

    private int _currentMiniGameIndex = -1;
    private MiniGameBase _currentMiniGameInstance;

    public GameFlowState CurrentState { get; private set; } = GameFlowState.None;

    private void Awake()
    {
        // 싱글톤 (필요 없으면 제거해도 됨)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 게임 시작 전 상태
        CurrentState = GameFlowState.WaitingToStart;

        // 여기서 바로 시작하거나, 타이틀 화면에서 버튼 누를 때 StartGame() 호출해도 됨
        StartGame();
    }

    public void StartGame()
    {
        _currentMiniGameIndex = -1;
        CurrentState = GameFlowState.PlayingMiniGame;

        GoToNextMiniGame();
    }

    private void GoToNextMiniGame()
    {
        // 이전 미니게임 정리
        if (_currentMiniGameInstance != null)
        {
            _currentMiniGameInstance.OnMiniGameEnded -= HandleMiniGameEnded;
            Destroy(_currentMiniGameInstance.gameObject);
            _currentMiniGameInstance = null;
        }

        _currentMiniGameIndex++;

        // 모든 미니게임 클리어
        if (_currentMiniGameIndex >= _miniGamePrefabs.Count)
        {
            CurrentState = GameFlowState.AllMiniGamesCleared;
            OnAllMiniGamesCleared();
            return;
        }

        // 다음 미니게임 생성
        var prefab = _miniGamePrefabs[_currentMiniGameIndex];
        _currentMiniGameInstance = Instantiate(prefab, _miniGameParent);
        _currentMiniGameInstance.Initialize(this);
        _currentMiniGameInstance.OnMiniGameEnded += HandleMiniGameEnded;

        _currentMiniGameInstance.StartMiniGame();
    }

    private void HandleMiniGameEnded(MiniGameBase miniGame, MiniGameResult result)
    {
        // 현재 진행 중인 미니게임이 아닌 경우 무시 (예외 상황 방지용)
        if (miniGame != _currentMiniGameInstance)
            return;

        switch (result)
        {
            case MiniGameResult.Cleared:
                // 다음 미니게임으로 진행
                GoToNextMiniGame();
                break;

            case MiniGameResult.Failed:
                // 실패 시 게임 오버 흐름으로
                CurrentState = GameFlowState.GameOver;
                OnMiniGameFailed();
                break;
        }
    }

    private void OnAllMiniGamesCleared()
    {
        Debug.Log("모든 미니게임 클리어! 엔딩 연출 또는 결과 화면 표시");

        // TODO: 엔딩 UI, 점수 표시 등 호출
        // e.g. UIManager.Instance.ShowAllClearPanel();
    }

    private void OnMiniGameFailed()
    {
        Debug.Log("미니게임 실패! 다시 플레이하시겠습니까? UI 표시");

        // TODO: 재시도 UI 열고, 버튼에서 아래 함수들 호출
        // -> RetryCurrentMiniGame()
        // -> ReturnToTitle() or StartGame() 등
    }

    // === UI 버튼에서 연결해서 쓸 수 있는 함수들 예시 ===

    public void RetryCurrentMiniGame()
    {
        if (CurrentState != GameFlowState.GameOver)
            return;

        // 실패했던 미니게임 인덱스를 유지한 채 다시 시작
        CurrentState = GameFlowState.PlayingMiniGame;

        // 인덱스를 하나 뒤로 돌린 후, GoToNextMiniGame()이 다시 같은 미니게임을 시작하도록
        _currentMiniGameIndex--;
        GoToNextMiniGame();
    }

    public void RestartWholeGame()
    {
        // 전체 게임을 처음부터 다시
        StartGame();
    }
}
