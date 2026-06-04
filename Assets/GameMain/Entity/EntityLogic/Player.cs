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

            float x = 0f;
            float y = 0f;

            if (Input.GetKey(KeyCode.LeftArrow)) x = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) x = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) y = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) y = -1f;

            Vector3 move = new Vector3(x, y, 0f);

            controller.Move(move * speed * Time.deltaTime);
        }
    }
}