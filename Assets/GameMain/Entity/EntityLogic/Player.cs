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
            Debug.Log("UPDATE RUN");

            if (controller == null)
            {
                Debug.Log("NO CONTROLLER");
                return;
            }

            bool left = Input.GetKey(KeyCode.LeftArrow);
            bool right = Input.GetKey(KeyCode.RightArrow);
            bool up = Input.GetKey(KeyCode.UpArrow);
            bool down = Input.GetKey(KeyCode.DownArrow);

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
            
            /*
            float x = 0f;
            float y = 0f;

        
            if (Input.GetKey(KeyCode.LeftArrow)) x = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) x = 1f;

            if (x == 0f)
            {
                if (Input.GetKey(KeyCode.UpArrow)) y = 1f;
                else if (Input.GetKey(KeyCode.DownArrow)) y = -1f;
            }
            */

            Vector3 move = new Vector3(x, y, 0f);

            controller.Move(move * speed * Time.deltaTime);
        }
    }
}