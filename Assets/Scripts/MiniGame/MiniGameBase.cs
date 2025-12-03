using System;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    // 이 미니게임이 종료될 때 (클리어 or 실패) 쏘는 이벤트
    public event Action<MiniGameBase, MiniGameResult> OnMiniGameEnded;

    // 필요하면 GameManager 참조를 저장해둘 수 있음
    protected GameManager _gameManager;

    public virtual void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    // 미니게임 시작 시 호출
    public abstract void StartMiniGame();

    // 미니게임 강제 종료(씬 전환, 게임 오버 등) 시 호출
    public abstract void StopMiniGame();

    // 자식 미니게임에서 클리어/실패 시 이 메서드를 호출
    protected void EndMiniGame(MiniGameResult result)
    {
        OnMiniGameEnded?.Invoke(this, result);
    }

    // 편의 함수
    protected void Clear()
    {
        EndMiniGame(MiniGameResult.Cleared);
    }

    protected void Fail()
    {
        EndMiniGame(MiniGameResult.Failed);
    }
}
