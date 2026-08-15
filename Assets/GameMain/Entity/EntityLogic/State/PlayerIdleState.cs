using UnityEngine;

namespace Monster
{
    // <summary>
    // Player의 대기 상태입니다.
    // 방향키 입력을 기다리고 입력이 들어오면 Move 상태로 전환합니다.
    // </summary>
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(Player player) : base(player)
        {

        }

        public override void Enter()
        {
            // Idle 상태로 들어오면 걷기 애니메이션을 종료합니다.
            mPlayer.SetMoveAnimation(false);
        }

        public override void Update()
        {
            {
                // 방향키 입력을 가져옵니다.
                Vector2 moveDirection = mPlayer.GetMoveDirection();

                // 입력이 없으면 Idle 상태를 유지합니다.
                if (moveDirection == Vector2.zero)
                {
                    return;
                }

                // 캐릭터가 바라보는 방향을 변경합니다.
                mPlayer.SetDirectionAnimation(moveDirection);

                // 이동 방향 저장
                mPlayer.mMoveDirection = moveDirection;

                // Move 상태로 전환
                mPlayer.ChangeState(mPlayer.MoveState);
            }
        }
    }
}