using TMPro;
using UnityEngine;

namespace Monster
{
    public class Warp : MonoBehaviour
    {
        public void WarpPlayer(Player player)
        {
            // mPlayer가 아니라 매개변수로 받은 player를 검사합니다.
            if (player == null)
            {
                Debug.LogWarning("Player를 찾을 수 없습니다.");

                return;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 여기서 실제로 워프 포인트에 들어온 Player를 가져옵니다.
            Player pㅣayer = collision.GetComponent<Player>();

            if (pㅣayer == null)
            {
                return;
            }

            // 위에서 찾은 Player를 WarpPlayer를 함수로 넘깁니다.
            WarpPlayer(pㅣayer);
        }
    }
}