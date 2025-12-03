using UnityEngine;

public class ExampleMiniGame : MiniGameBase
{
    [Header("Example Settings")]
    [SerializeField] private float _autoClearTime = 5f;

    private float _timer;
    private bool _isRunning = false;

    public override void StartMiniGame()
    {
        Debug.Log("ExampleMiniGame 시작!");

        _timer = 0f;
        _isRunning = true;

        // 여기서 플레이어 세팅, UI 열기 등 게임 시작 준비를 하면 됨
    }

    public override void StopMiniGame()
    {
        Debug.Log("ExampleMiniGame 강제 종료");
        _isRunning = false;

        // 필요하면 정리 로직
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        _timer += Time.deltaTime;

        // 1) 자동 클리어 조건 예시
        if (_timer >= _autoClearTime)
        {
            Debug.Log("ExampleMiniGame 클리어 조건 달성!");
            _isRunning = false;
            Clear(); // MiniGameBase의 함수 -> Event 발생
        }

        // 2) 실패 조건 예시 (스페이스바 입력)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("ExampleMiniGame 실패 조건 발생 (스페이스바)");
            _isRunning = false;
            Fail(); // Event 발생
        }
    }
}
