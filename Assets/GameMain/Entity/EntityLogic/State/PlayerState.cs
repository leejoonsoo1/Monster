namespace Monster
{
    // <summary>
    // Player의 모든 샅애가 상속받는 기본 State 클래스입니다.
    // </summary>
    public abstract class PlayerState
    {
        protected Player mPlayer;
        protected PlayerState(Player player)
        {
            mPlayer = player;
        }

        // <summary>
        // 해당 상태로 진입했을 때 한 번 호출됩니다.
        // </summary>
        public virtual void Enter()
        {

        }

        // <summary>
        // 해당 상태에 머무르는 동안 매 프레임 호출됩니다.
        // </summary>
        public virtual void Update()
        {

        }

        // <summary>
        // 해당 상태에서 빠져나갈 때 한 번 호출됩니다.
        // </summary>
        public virtual void Exit()
        {

        }
    }
}