using UnityEngine;

public static class AnimatorExtensions
{
    private static readonly int HashPosX = Animator.StringToHash("PosX");
    private static readonly int HashPosY = Animator.StringToHash("PosY");
    private static readonly int HashIsMoving = Animator.StringToHash("isMoving");
    private static readonly int HashJumpTrigg = Animator.StringToHash("JumpTrigg");

    // 이동 입력 업데이트
    public static void SetMoveInput(this Animator animator, Vector2 input)
    {
        animator.SetFloat(HashPosX, input.x);
        animator.SetFloat(HashPosY, input.y);

        // 움직임 여부는 sqrMagnitude로 판단(성능 좋음)
        bool moving = input.sqrMagnitude > 0.001f;
        animator.SetBool(HashIsMoving, moving);
    }

    // 점프 이벤트
    public static void DoJump(this Animator animator)
    {
        animator.SetTrigger(HashJumpTrigg);
    }
}
