using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityGameFramework.Runtime;
using static Monster.TilemapManager;

namespace Monster
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class Player : TargetTableObject
    {
        [Header("이동설정")]
        public float mSpeed = 9f;

        private Rigidbody2D mRigidbody2D;
        private PlayerData mPlayerData;
        private Animator mAnimator;

        // 셀 좌표와 셀 중앙을 계산할 기준 Grid입니다
        private Grid mGrid;
        private TilemapManager mTilemapManager;

        // 현재 한 칸 이동 중인지 확인합니다.
        private bool mIsMoving = false;

        // 현재 Player가 실행 중인 상태입니다.
        private PlayerState mCurrentState;

        // 코루틴 제어 변수
        private Coroutine mMoveCoroutine;

        private int mMoveVersion = 0;

        private Vector2 mTargetPosition;

        // Player의 대기 상태입니다.
        public PlayerIdleState IdleState { get; private set; }

        // Player의 이동 상태입니다.
        public PlayerMoveState MoveState { get; private set; }

        // Idle 상태에서 입력받은 이동 방향을 
        // Move 상태에서 사용하기 위해 보관합니다.
        public Vector2 mMoveDirection { get; set; }

        // MoveState에서 현재 이동 완료 여부를 확인하기 위한 프로퍼티입니다.
        public bool IsMoving => mIsMoving;

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
                Log.Warning("RIgidbody2D가 없습니다.");

                return;
            }

            // 현재 씬에 있는 Grid 컴포넌트를 찾습니다.
            mGrid = Object.FindAnyObjectByType<Grid>();

            if (mGrid == null)
            {
                Log.Warning("Grid를 찾을 수 없습니다.");

                return;
            }

            // 현재 씬에 있는 TilemapManager를 찾습니다.
            mTilemapManager = RPGGame.Instance.TilemapManager;

            if (mTilemapManager == null)
            {
                Log.Warning("TilemapManager를 찾을 수 없습니다.");

                return;
            }

            // 스폰 위치가 속한 셀을 계산합니다.
            Vector3Int spawnCell = mGrid.WorldToCell(mPlayerData.Position);

            // 스폰 셀의 중앙 좌표를 가져옵니다.
            Vector3 spawnCenter = mGrid.GetCellCenterWorld(spawnCell);

            // 플레이어를 셀 중앙에 생성합니다.
            transform.position = new Vector3(spawnCenter.x, spawnCenter.y, transform.position.z);

            // Rigidbody2D의 위치도 동일하게 맞춥니다.
            mRigidbody2D.position = new Vector2(spawnCenter.x, spawnCenter.y);

            // 생성 시 이동 상태를 초기화합니다.
            mIsMoving = false;

            // Player가 사용할 상태 객체를 생성합니다.
            IdleState = new PlayerIdleState(this);
            MoveState = new PlayerMoveState(this);

            // Player 생성 직후에는 Idle 상태로 시작합니다.
            ChangeState(IdleState);
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (mRigidbody2D == null)
            {
                return;
            }

            if (mGrid == null)
            {
                return;
            }

            // Player가 직접 입력과 이동 상태를 처리하지 않고
            // 현재 State가 자신의 행동을 처리합니다.
            mCurrentState?.Update();
        }

        // <summary>
        // Player의 현재 State를 변경합니다.
        // </summary>
        public void ChangeState(PlayerState newState)
        {
            if (newState == null)
            {
                return;
            }

            // 기존 상태에서 빠져나갑니다.
            mCurrentState?.Exit();

            // 새로운 상태를 현재 상태로 설정합니다.
            mCurrentState = newState;

            // 새로운 상태에 진입합니다.
            mCurrentState.Enter();
        }

        // <summary>
        // 현재 눌린 방향키를 확인하고 이동 방향을 반환합니다.
        // </summary>
        public Vector2 GetMoveDirection()
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
        // 현재 저장된 MoveDirection을 이용하여
        // 바로 앞 타일 중앙으로 한 칸 이동을 시작합니다.
        // </summary>
        public void StartTileMove()
        {
            // 이미 이동하고 있다면 중복 이동을 실행하지 않습니다.
            if (mIsMoving)
            {
                return;
            }

            if (mMoveDirection == Vector2.zero)
            {
                return;
            }

            // 현재 플레이어가 위치한 셀을 구합니다.
            Vector3Int currentCell = mGrid.WorldToCell(mRigidbody2D.position);

            // 입력 방향 바로 앞의 셀을 계산합니다.
            Vector3Int targetCell = currentCell + new Vector3Int(Mathf.RoundToInt(mMoveDirection.x), Mathf.RoundToInt(mMoveDirection.y), 0);

            List<EBlockedTileType> blockedTIleTypes = mTilemapManager.GetBlockedTileTypes(targetCell);

            if (blockedTIleTypes.Count > 0)
            {
                mIsMoving = false;

                return;
            }

            // 목표 셀의 정확한 중앙 좌표를 가져옵니다.
            Vector3 targetCellCenter = mGrid.GetCellCenterWorld(targetCell);

            // Vector3를 Vector2 좌표를 전환합니다.
            mTargetPosition = new Vector2(targetCellCenter.x, targetCellCenter.y);

            // 새로운 이동을 시작할 때 이동 번호를 증가시킵니다.
            mMoveVersion++;

            // 이번 이동의 번호를 코루틴으로 전달합니다.
            StartCoroutine(MoveToCell(mTargetPosition, mMoveVersion));
        }

        // <summary>
        // 이동 방향을 Animator에 전달합니다.
        // </summary>
        public void SetDirectionAnimation(Vector2 moveDirection)
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
        public void SetMoveAnimation(bool isMoving)
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
        private IEnumerator MoveToCell(Vector2 targetPosition, int moveVersion)
        {
            // 이동이 끝날 때까지 다른 이동을 막습니다.
            mIsMoving = true;

            while (Vector2.Distance(mRigidbody2D.position, mTargetPosition) > 0.01f)
            {
                // 워프 등으로 이동 번호가 바뀌었다면
                // 이 코루틴은 과거 이동이므로 즉시 종료합니다.
                if (moveVersion != mMoveVersion)
                {
                    yield break;
                }

                Vector2 nextPosition = Vector2.MoveTowards(mRigidbody2D.position, mTargetPosition, mSpeed * Time.fixedDeltaTime);

                mRigidbody2D.MovePosition(nextPosition);

                yield return new WaitForFixedUpdate();
            }

            // 마지막 순간에도 기존 이동이 취소됐는지 확인합니다.
            if (moveVersion != mMoveVersion)
            {
                yield break;
            }

            mRigidbody2D.position = targetPosition;

            mIsMoving = false;
        }
    }
}