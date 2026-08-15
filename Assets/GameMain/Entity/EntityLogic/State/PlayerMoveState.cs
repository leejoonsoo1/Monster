using UnityEngine;
namespace Monster
{
    // <summary>
    // Player가 한 타일을 이동하는 상태입니다.
    // </summary>
    public class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(Player player) : base (player)
        { 
        }

        public override void Enter()
        {
            // Move 상태에 들어오면 걷기 애니메이션을 시작합니다.
            mPlayer.SetMoveAnimation(true);

            // 한 칸 이동 시작
            mPlayer.StartTileMove();
        }

        public override void Update()
        {
            // 아직 타일 중앙까지 이동하고 있다면 Move 상태를 유지합니다.
            if (mPlayer.IsMoving)
            {
                return;
            }

            // 한 칸 이동이 끝났을 때
            // 키가 계속 눌려있는지 확인합니다.
            Vector2 moveDirection = mPlayer.GetMoveDirection();

            // 키를 뗐을 때만 Idle로 돌아갑니다.
            if (moveDirection == Vector2.zero)
            {
                mPlayer.ChangeState(mPlayer.IdleState);

                return;
            }

            // 키를 계속 누르고 있으면 MoveState를 유지합니다.
            mPlayer.mMoveDirection = moveDirection;

            // 방향이 바뀌었을 수도 있으므로 갱신합니다.
            mPlayer.SetDirectionAnimation(moveDirection);

            // 다음 한 칸 이동을 바로 시작합니다.
            mPlayer.StartTileMove();
        }

        public override void Exit()
        {
            // Move 상태가 끝나면 걷기 애니메이션을 종료합니다.
            mPlayer.SetMoveAnimation(false);
        }
    }
}