using EpicToonFX;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;

namespace Monster
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : TargetTableObject
    {
        private CharacterController controller;
        private PlayerData playerData;
        private Animator animator;

        private enum MoveAxis
        { 
            None,
            Horizontal,
            Vertical
        }

        private MoveAxis currentAxis = MoveAxis.None;

        public float speed = 5f;

        protected internal override void OnShow(object userData)
        {
            base.OnShow(userData);

            playerData = userData as PlayerData;
            animator = GetComponent<Animator>();

            if (playerData == null)
            {
                Log.Warning("PlayerData Invalid");
                return;
            }

            controller = GetComponent<CharacterController>();

            transform.position = playerData.Position;
        }

       public void Update()
        {
            // CharacterController가 없으면 이동 처리 중단
            if (controller == null)
            {
                return;
            }

            // 이동 입려값
            float x = 0f;
            float y = 0f;

            // 한 번에 한 방향만 이동하도록 우선순위 적용
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                x = -1f;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                x = 1f;
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                y = 1f;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                y = -1f;
            }

            // 이동 중인지 여부
            bool isMove = (x > 0f || x < 0f || y < 0f || y > 0f);
            if (animator != null)
            {
                // 이동 중일 때만 마지막 바라보는 방향 계산
                if (isMove)
                {
                    animator.SetFloat("MoveX", x);
                    animator.SetFloat("MoveY", y);
                }

                // 이동 여부 전달 (Ide <-> Wlak 전환)
                animator.SetBool("IsMove", isMove);
            }

            // 이동 방향 생성
            Vector3 move = new Vector3(x, y, 0f).normalized;

            // CharacterController를 이용한 이동 처리
            controller.Move(move * speed * Time.deltaTime);

            /*
            //Debug.Log("UPDATE RUN");

            if (controller == null)
            {
                //Debug.Log("NO CONTROLLER");
                return;
            }

            bool left   = Input.GetKey(KeyCode.LeftArrow);
            bool right  = Input.GetKey(KeyCode.RightArrow);
            bool up     = Input.GetKey(KeyCode.UpArrow);
            bool down   = Input.GetKey(KeyCode.DownArrow);

            // 현재 축의 키를 모두 떼면 축 초기화
            if (currentAxis == MoveAxis.Horizontal && !left && !right)
            {
                currentAxis = MoveAxis.None;
            }
            else if (currentAxis == MoveAxis.Vertical && !up && !down)
            {
                currentAxis = MoveAxis.None;
            }

            // 아직 이동 축이 없을 때만 새 축 결정
            if (currentAxis == MoveAxis.None)
            {
                if (left || right)
                {
                    currentAxis = MoveAxis.Horizontal;
                }
                else if (up || down)
                {
                    currentAxis = MoveAxis.Vertical;
                }

            }

            float x = 0f;
            float y = 0f;

            if (currentAxis == MoveAxis.Horizontal)
            {
                if (left) x = -1f;
                else if (right) x = 1f;
            }
            else if (currentAxis == MoveAxis.Vertical)
            {
                if (up) y = 1f;
                else if (down) y = -1f;
            }

            Vector3 move = new Vector3(x, y, 0f).normalized;

            // 애니메이션 처리
            if (animator != null)
            {
                bool isMove = (x != 0f || y != 0f);

                if (isMove)
                {
                    animator.SetFloat("MoveX", x);
                    animator.SetFloat("MoveY", y);
                }

                animator.SetBool("IsMove", isMove);
            }

            controller.Move(move * speed * Time.deltaTime);
            */
        }
    }
}