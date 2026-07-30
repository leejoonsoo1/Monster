using UnityEngine;

namespace Monster
{
    public class CameraFollow : MonoBehaviour
    {
        // 따라갈 대상
        public Transform mTarget;

        // 카메라와 대상 사이의 거리
        public Vector3 mOffset = new Vector3(0f, 0f, -20f);

        // 카메라 이동 속도
        public float mFollowSpeed = 10f;

        private void LateUpdate()
        {
            // 현재 스크립트가 실제로 실행되는지 확인
            //Debug.Log("CameraFollow LateUpdate 실행");

            if (mTarget == null)
                return;

            Vector3 targetPosition = mTarget.position + mOffset;

            //Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.position = new Vector3(mTarget.position.x, mTarget.position.y, -10f);
        }
    }
}

