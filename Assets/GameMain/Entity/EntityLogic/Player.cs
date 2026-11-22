using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.AI;
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

        [Header("절벽 점프 설정")]
        // 점프를 시작해서 착지하기까지 걸리는 시간입니다. 단위는 초입니다.
        [SerializeField, Min(0.02f)] private float mJumpDuration = 0.35f;
        // 점프 중 캐릭터 그림이 바닥에서 떠오르는 최대 높이입니다.
        [SerializeField, Min(0f)] private float mJumpHeight = 0.6f;

        // Animator와 SpriteRenderer가 들어 있는 자식 오브젝트입니다.
        [SerializeField] private Transform mVisual;
        // 점프가 끝난 뒤 캐릭터 그림을 되돌릴 위치입니다.
        private Vector3 mVisualRestPosition;

        private Rigidbody2D mRigidbody2D;
        private PlayerData  mPlayerData;
        private Animator    mAnimator;

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
            mAnimator       = GetComponentInChildren<Animator>();
            mRigidbody2D    = GetComponent<Rigidbody2D>();

            // 본체가 연결되어 있으면 카메라와 충돌 범위도 같이 움직이므로 연결을 해제합니다.
            if (mVisual == transform)
            {
                mVisual = null;
            }

            // Visual이 연결되지 않았다면 Animator가 붙은 자식 오브젝트를 사용합니다.
            if (mVisual == null && mAnimator != null && mAnimator.transform != transform)
            {
                mVisual = mAnimator.transform;
            }

            // 착지할 때 복원할 수 있도록 캐릭터 그림의 기본 위치를 저장합니다.
            if (mVisual != null)
            {
                mVisualRestPosition = mVisual.localPosition;
            }

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

            // PlayerData로 전달받은 방향을
            // 현재 Player의 이동 방향으로 저장합니다.
            mMoveDirection = mPlayerData.Direction;

            // Animator에도 방향을 전달하여
            // 실제 캐릭터가 저장했던 방향을 바라보도록 합니다.
            SetDirectionAnimation(mMoveDirection);

            // 생성 시 이동 상태를 초기화합니다.
            mIsMoving = false;

            // Player가 사용할 상태 객체를 생성합니다.
            IdleState = new PlayerIdleState(this);
            MoveState = new PlayerMoveState(this);

            // Player 생성 직후에는 Idle 상태로 시작합니다.
            ChangeState(IdleState);
        }

        // <summary>
        // Player가 회수될 때 진행 중인 이동을 취소하고 캐릭터 그림의 위치를 복원합니다.
        // </summary>
        protected internal override void OnHide(bool isShutdown, object userData)
        {
            // 이동 번호를 변경하여 이전 이동 코루틴이 착지 처리를 하지 않도록 합니다.
            mMoveVersion++;
            mIsMoving = false;
            ResetJumpVisual();
            base.OnHide(isShutdown, userData);
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

            // Battle 전환을 기다리는 동안에는
            // 추가 이동 입력을 받지 않습니다.
            if (RPGGame.Instance != null &&
                RPGGame.Instance.IsBattleRequested)
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

            Vector3Int moveCell = new Vector3Int(Mathf.RoundToInt(mMoveDirection.x), Mathf.RoundToInt(mMoveDirection.y), 0);

            // 현재 플레이어가 위치한 셀을 구합니다.
            Vector3Int currentCell  = mGrid.WorldToCell(mRigidbody2D.position);

            // 입력 방향 바로 앞의 셀을 계산합니다.
            Vector3Int targetCell   = currentCell + moveCell;

            // 목표 셀에 있는 모든 이동 불가 타일 종류를 가져옵니다.
            List<EBlockedTileType> blockedTileTypes = mTilemapManager.GetBlockedTileTypes(targetCell);

            // =========================================
            // 아래 방향으로 이동할 때 앞 칸의 장애물이 Hill뿐이면 점프합니다.
            // =========================================
            if (mMoveDirection == Vector2.down &&
                blockedTileTypes.Count == 1 && blockedTileTypes[0] == EBlockedTileType.Hill)
            {
                // Hill 타일을 넘어 두 칸 앞에 착지합니다.
                Vector3Int landingCell = targetCell + moveCell;

                // 착지할 타일이 없거나 장애물이 있으면 이동을 시작하지 않습니다.
                if (!mTilemapManager.IsWalkable(landingCell))
                {
                    mIsMoving = false;

                    return;
                }

                Vector3 landingCenter = mGrid.GetCellCenterWorld(landingCell);
                Vector2 landingPosition = new Vector2(landingCenter.x, landingCenter.y);

                mMoveVersion++;

                StartCoroutine(JumpToCell(landingPosition, mMoveVersion));

                return;
            }

            // =========================================
            // 기존 일반 이동 불가 검사
            // =========================================
            if (blockedTileTypes.Count > 0)
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

            // 점프 뒤에 다시 걸을 때 걷기 애니메이션을 재생합니다.
            SetMoveAnimation(true);

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

        // </summary>
        // Player가 한 칸 이동을 완료했을 때
        // 현재 위치에서 수풀을 밟은 횟수를 검사합니다.
        // </summary>
        private void CheckEncounter()
        {
            if (RPGGame.Instance == null)
            {
                return;
            }

            if (RPGGame.Instance.EncounterManager == null)
            {
                return;
            }

            // 현재 Player가 위치한 Grid Cell을 가져옵니다.
            Vector3Int currentCell = mGrid.WorldToCell(mRigidbody2D.position);

            // 현재 셀이 수풀인지 확인하고
            // 수풀 위에서 다섯 번째 이동을 완료했는지 판정합니다.
            bool isEncounter = RPGGame.Instance.EncounterManager.TryEncounter(currentCell);

            //인카운터가 발생하지 않았다면
            // 그대로 필드 이동을 계속합니다.
            if (isEncounter != true)
            {
                return;
            }

            Log.Info($"Player : Grass Encounter -"  +
                $"Cell = {currentCell}, " +
                $"Direction = {mMoveDirection}");

            RPGGame.Instance.RequestBattle();
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

            CompleteTileMove(targetPosition);
        }

        // <summary>
        // 본체를 착지 셀까지 이동시키고 캐릭터 그림을 포물선 궤적으로 움직입니다.
        // </summary>
        private IEnumerator JumpToCell(Vector2 landingPosition, int moveVersion)
        {
            // 착지할 때까지 새로운 이동이 시작되지 않도록 합니다.
            mIsMoving = true;

            Vector2 startPosition = mRigidbody2D.position;

            // 점프 시간은 최소 한 번의 물리 갱신만큼 확보합니다.
            float duration = Mathf.Max(mJumpDuration, Time.fixedDeltaTime);
            float elapsedTime = 0f;
            WaitForFixedUpdate waitForPhysics = new WaitForFixedUpdate();

            // 걷기 애니메이션을 멈추고 아래를 바라보는 대기 자세로 바로 전환합니다.
            SetDirectionAnimation(Vector2.down);
            SetMoveAnimation(false);
            if (mAnimator != null)
            {
                mAnimator.Play("Base Layer.Idle", 0, 0f);
            }

            // 부모의 배율과 관계없이 설정한 높이만큼 올라가도록 로컬 좌표로 변환합니다.
            Vector3 localJumpHeight = Vector3.zero;
            if (mVisual != null)
            {
                localJumpHeight = mVisual.parent.InverseTransformVector(Vector3.up * mJumpHeight);
            }

            while (elapsedTime < duration)
            {
                // 진행률에 맞춰 본체를 착지 지점으로 이동시키고 물리 갱신을 기다립니다.
                elapsedTime += Time.fixedDeltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);
                mRigidbody2D.MovePosition(Vector2.Lerp(startPosition, landingPosition, progress));
                yield return waitForPhysics;

                if (moveVersion != mMoveVersion)
                {
                    // 이동이 취소된 경우 높이 보정만 되돌리고 착지 처리는 건너뜁니다.
                    ResetJumpVisual();
                    yield break;
                }

                if (mVisual != null)
                {
                    // 점프의 중간에서 가장 높이 올라가고 착지할 때 기본 높이로 돌아옵니다.
                    float arc = 4f * progress * (1f - progress);
                    mVisual.localPosition = mVisualRestPosition + localJumpHeight * arc;
                }
            }

            // 캐릭터 그림을 원래 높이로 돌린 뒤 도착한 셀을 처리합니다.
            ResetJumpVisual();
            CompleteTileMove(landingPosition);
        }

        // <summary>
        // 점프로 이동한 캐릭터 그림을 저장해 둔 위치로 되돌립니다.
        // </summary>
        private void ResetJumpVisual()
        {
            if (mVisual != null)
            {
                mVisual.localPosition = mVisualRestPosition;
            }
        }

        // <summary>
        // 걷기와 점프가 끝나면 도착한 셀을 한 번만 처리합니다.
        // </summary>
        private void CompleteTileMove(Vector2 targetPosition)
        {
            // Rigidbody와 Transform을 셀 중앙에 맞춰 배틀 전환 시에도 도착 위치를 저장할 수 있게 합니다.
            mRigidbody2D.position = targetPosition;
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

            // 현재 타일맵을 갱신하고 도착한 셀에서 수풀 인카운터를 확인합니다.
            mTilemapManager?.RefreshTilemaps();
            CheckEncounter();
            mIsMoving = false;
        }
    }
}
