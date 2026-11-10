using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    // EntityComponent에서 전투 포켓몬을 생성하고 회수합니다.
    public class BattlePokemonSpawner : GameFrameworkComponent
    {
        private const string mEntityGroupName = "BattlePokemon";

        [SerializeField] private Transform mEnemySpawnPoint;

        [SerializeField] private Transform mPlayerSpawnPoint;

        [SerializeField] private string mEnemyPokemonAssetName      = "EEVEE_0";

        [SerializeField] private string mPlayerPokemonAssetName     = "PIKACHU_0";

        [SerializeField] private Vector2 mEnemyViewportPosition     = new Vector2(0.78f, 0.63f);

        [SerializeField] private Vector2 mPlayerViewportPosition    = new Vector2(0.22f, 0.43f);

        [SerializeField] private float mCameraDistance              = 10f;

        [SerializeField] private float mPokemonScale                = 1.5f;

        private readonly List<int> mEntityIds                       = new List<int>();

        private EntityComponent mEntityComponent;

        private CameraFollow mCameraFollow;

        private bool mCameraFollowWasEnabled;

        public int EnemyEntityId    { get; private set; }

        public int PlayerEntityId   { get; private set; }

        public void SpawnBattlePokemon()
        {
            DespawnBattlePokemon();

            mEntityComponent    = GameEntry.GetComponent<EntityComponent>();
            Camera battleCamera = Camera.main;

            if (mEntityComponent == null || battleCamera == null)
            {
                Log.Error("BattlePokemonSpawner : EntityComponent 또는 Main Camera가 없습니다.");

                return;
            }

            if (!mEntityComponent.HasEntityGroup(mEntityGroupName) &&
                !mEntityComponent.AddEntityGroup(mEntityGroupName, 60f, 16, 60f, 0))
            {
                Log.Error("BattlePokemon Entity Group 생성에 실패했습니다.");

                return;
            }

            // 숨겨진 필드 캐릭터를 따라 카메라가 움직이지 않게 합니다.
            mCameraFollow = battleCamera.GetComponent<CameraFollow>();

            if (mCameraFollow != null)
            {
                mCameraFollowWasEnabled = mCameraFollow.enabled;
                mCameraFollow.enabled   = false;
            }

            Vector3 enemyPosition   = GetSpawnPosition(battleCamera, mEnemySpawnPoint, mEnemyViewportPosition);
            Vector3 playerPosition  = GetSpawnPosition(battleCamera, mPlayerSpawnPoint, mPlayerViewportPosition);

            EnemyEntityId           = EntitySerialId.Next();
            PlayerEntityId          = EntitySerialId.Next();

            SpawnPokemon(new BattlePokemonData(EnemyEntityId, 133, enemyPosition, null, true, mPokemonScale), enemyPosition);
            SpawnPokemon(new BattlePokemonData(PlayerEntityId, 25, playerPosition, null, false, mPokemonScale), playerPosition);
        }

        private Vector3 GetSpawnPosition(Camera camera, Transform spawnPoint, Vector2 viewportPosition)
        {
            return spawnPoint != null
                ? spawnPoint.position
                : camera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, mCameraDistance));
        }

        private void SpawnPokemon(BattlePokemonData pokemonData, Vector3 spawnPosition)
        {
            if (pokemonData == null)
            {
                Log.Error("BattlePokemonData is null.");

                return;
            }

            string assetName = pokemonData.IsEnemy ? mEnemyPokemonAssetName : mPlayerPokemonAssetName;

            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Battle Pokemon의 Addressable 이름이 비어 있습니다.");

                return;
            }

            pokemonData.Position = spawnPosition;

            // 로딩 중에도 종료 요청으로 취소할 수 있도록 먼저 ID를 기록합니다.
            mEntityIds.Add(pokemonData.Id);

            mEntityComponent.ShowEntity(pokemonData.Id, typeof(BattlePokemon), assetName, mEntityGroupName, pokemonData);
        }

        public void DespawnBattlePokemon()
        {
            if (mEntityComponent != null)
            {
                foreach (int entityId in mEntityIds)
                {
                    if (mEntityComponent.HasEntity(entityId) || mEntityComponent.IsLoadingEntity(entityId))
                    {
                        mEntityComponent.HideEntity(entityId);
                    }
                }
            }

            mEntityIds.Clear();

            EnemyEntityId   = 0;
            PlayerEntityId  = 0;

            if (mCameraFollow != null)
            {
                mCameraFollow.enabled   = mCameraFollowWasEnabled;
                mCameraFollow           = null;
            }
        }
    }
}