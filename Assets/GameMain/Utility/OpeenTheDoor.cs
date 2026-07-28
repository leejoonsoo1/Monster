using System.Collections;
using UnityEngine;

namespace Monster
{
    public class OpeenTheDoor : MonoBehaviour
    {
        [SerializeField] private Door door;
        [SerializeField] private int waitTime = 1;

        // 문 앞에 플레이어가 있는지 판단하는 변수입니다.
        private bool isPlayerInRange;
        private bool open = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() != null)
            {
                isPlayerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() != null)
            {
                open = false;

                isPlayerInRange = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isPlayerInRange)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) && open == false)
            {
                open = true;
                door.ColliderOff();
                StartCoroutine(OpenDoor());
            }
        }

        private IEnumerator OpenDoor()
        {
            yield return new WaitForSeconds(waitTime);

            Quaternion startRotation    = door.transform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, -90, 15);

            float time = 0f;

            while (time < 1f)
            {
                time += Time.deltaTime;

                door.transform.rotation = Quaternion.Lerp(startRotation, endRotation, time / 1f);

                yield return null;
            }

            door.transform.rotation = endRotation;
        }
    }
}