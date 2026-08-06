using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityGameFramework.Runtime;

namespace Monster
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class Player : TargetTableObject
    {
        [Header("이동 설정")]
        public float mSpeed = 5f;

        private Rigidbody2D     mRigidbody2D;
        private PlayerData      mPlayerData;
        private Animator        mAnimator;

        // 셀 좌표와 셀 중앙을 계산할 기준 Grid입니다.
        private Grid mGrid;

        // 현재 한 칸 이동 중인지 확인합니다.
        private bool mIsMoving = false;

        protected internal override void OnShow(object userData)
        {
            base.OnShow(userData);

            mPlayerData     = userData as PlayerData;
            mAnimator       = GetComponent<Animator>();
            mRigidbody2D    = GetComponent<Rigidbody2D>();

            if (mPlayerData == null)
            {
                Log.Warning("PlayerData Invalid");

                return;
            }

            if (mRigidbody2D == null)
            {
                Log.Warning("Rigidbody2D가 없습니다.");

                return;
            }

            // 플레이어의 초기 위치를 설정합니다.
            transform.position = mPlayerData.Position;

            // 현재 씬에 있는 Grid 컴포넌트를 찾습니다.
            mGrid = Object.FindAnyObjectByType<Grid>();

            if (mGrid == null)
            {
                Log.Warning("Grid를 찾을 수 없습니다.");

                return;
            }

            // 플레이어의 현재 월드 좌표를 셀 좌표로 변환합니다.
            Vector3Int currentCell = mGrid.WorldToCell(transform.position);

            // 플레이어를 현재 셀의 정확한 중앙으로 보정합니다.
            Vector3 currentCellCenter = mGrid.GetCellCenterWorld(currentCell);

            mRigidbody2D.position = new Vector2(currentCellCenter.x, currentCellCenter.y);

            // 생성 시 이동 상태를 초기화합니다.
            mIsMoving = false;
        }

       public void Update()
        {
            if (mRigidbody2D == null)
            {
                return;
            }

            // 이동 중에는 입력을 받지 않습니다.
            if (mIsMoving)
            {
                return;
            }

            // 방향키 입력을 셀 이동 방향으로 가져옵니다.
            Vector2 moveDirection = GetMoveDirection();

            // 입력이 없으면 대기 애니메이션으로 전환합니다.
            if (moveDirection == Vector2.zero)
            {
                SetMoveAnimation(false);

                return;
            }

            // 이동하지 못하더라도 바라보는 방향은 변경합니다.
            SetDirectionAnimation(moveDirection);

            // 현재 플레이어가 위치한 셀을 구합니다.
            Vector3Int currentCell = mGrid.WorldToCell(mRigidbody2D.position);

            // 입력 방향 바로 앞의 셀을 계산합니다.
            Vector3Int targetCell = currentCell + new Vector3Int(Mathf.RoundToInt(moveDirection.x), Mathf.RoundToInt(moveDirection.y), 0);

            // 이동할 셀의 정확한 중앙 월드 좌표를 가져옵니다.
            Vector3 targetCellCenter = mGrid.GetCellCenterWorld(targetCell);

            Vector2 targetPosition = new Vector2(targetCellCenter.x, targetCellCenter.y);

            // 목표 타일 중앙으로 이동을 시작합니다.
            StartCoroutine(MoveToCell(targetPosition));
        }

        // <summary>
        // 현재 눌린 방향키를 확인하고 이동 방향을 변환합니다.
        // 한 번에 한 방향만 처리합니다.   
        // </summary>
        private Vector2 GetMoveDirection()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                return Vector2.up;
            }
           
            if (Input.GetKey(KeyCode.DownArrow))
            {
                return Vector2.down;
            }
            
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                return Vector2.left;
            }
          
            if (Input.GetKey(KeyCode.RightArrow))
            {
                return Vector2.right;
            }

            return Vector2.zero;
        }

        // <summary>
        // 이동 방향과 이동 여부를 Animator에 전달합니다.
        // </summary>
        private void SetDirectionAnimation(Vector2 moveDirection)
        {
            if (mAnimator == null)
            {
                return;
            }

            mAnimator.SetFloat("MoveX", moveDirection.x);
            mAnimator.SetFloat("MoveY", moveDirection.y);
        }
            // <summary>
            // 걷기 애니메이션의 재생 여부를 설정합니다.
            // </summary>
        private void SetMoveAnimation(bool isMoving)
        {
            if (mAnimator == null)
            {
                return;
            }
                mAnimator.SetBool("IsMove", isMoving);
        }

        // <summary>
        // 플레이어를 목표 타일의 중앙까지 부드럽게 이동합니다.
        // </summary>
        private IEnumerator MoveToCell(Vector2 targetPosition)
        {
            // 이동이 끝날 때까지 다른 입력을 막습니다.
            mIsMoving = true;

            while (Vector2.Distance(mRigidbody2D.position, targetPosition) > 0.01f)
            {
                Vector2 nextPosition = Vector2.MoveTowards(mRigidbody2D.position, targetPosition, mSpeed * Time.fixedDeltaTime);

                mRigidbody2D.MovePosition(nextPosition);

                // Rigidbody2D 의 물리 갱신 주기에 맞춰 이동합니다.
                yield return new WaitForFixedUpdate();
            }

            // 미세한 좌표 오차가 남지 않도록 정확히 고정합니다.
            mRigidbody2D.position = targetPosition;

            // 이동이 끝났으므로 대기 애니메이션으로 전환합니다.
            SetMoveAnimation(false);

            // 다음 이동 입력을 받을 수 있도록 상태를 해제합니다.
            mIsMoving = false;
        }
    }
}