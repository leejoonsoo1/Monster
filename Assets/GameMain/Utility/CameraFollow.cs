using UnityEngine;

namespace Monster
{
    public class CameraFollow : MonoBehaviour
    {
        // 따라갈 대상
        public Transform target;

        // 카메라와 대상 사이의 거리
        public Vector3 offset = new Vector3(0f, 0f, -20f);

        // 카메라 이동 속도
        public float followSpeed = 10f;

        private void LateUpdate()
        {
            // 현재 스크립트가 실제로 실행되는지 확인
            //Debug.Log("CameraFollow LateUpdate 실행");

            if (target == null)
                return;

            Vector3 targetPosition = target.position + offset;

            //Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.position = new Vector3(target.position.x, target.position.y, -10f);
        }
    }
}

