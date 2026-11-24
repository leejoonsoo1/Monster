using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    // EntityComponent에서 전투 포켓몬을 생성하고 회수합니다.
    public class BattlePokemonSpawner : GameFrameworkComponent
    {
        private const string mEntityGroupName = "BattlePokemon";

        // 포켓몬 번호와 아군용 프리팹 주소를 연결합니다.
        [Serializable]
        private class PokemonPrefabMapping
        {
            [SerializeField] private int mPokemonId;
            [SerializeField] private string mAssetName;

            public int PokemonId
            {
                get { return mPokemonId; }
            }

            public string AssetName
            { 
                get { return mAssetName; }
            }
        }

        [SerializeField] private Transform mEnemySpawnPoint;

        [SerializeField] private Transform mPlayerSpawnPoint;

        [SerializeField] private string mEnemyPokemonAssetName = "EEVEE_0";

        // 외부에서 전달받은 상대 포켓몬 번호를 보관합니다. 기본값은 이브이입니다.
        [SerializeField, Min(1)] private int mEnemyPokemonId = 133;

        [SerializeField] private string mPlayerPokemonAssetName = "PIKACHU_0";

        // 피카츄 이외의 아군 포켓몬 프리팹을 등록합니다.
        [SerializeField] private PokemonPrefabMapping[] mPlayerPokemonPrefabs;

        [SerializeField] private Vector2 mEnemyViewportPosition = new Vector2(0.78f, 0.63f);

        [SerializeField] private Vector2 mPlayerViewportPosition = new Vector2(0.22f, 0.43f);

        [SerializeField] private float mCameraDistance = 10f;

        [SerializeField] private float mPokemonScale = 1.5f;

        private readonly List<int> mEntityIds = new List<int>();

        private EntityComponent mEntityComponent;

        private CameraFollow mCameraFollow;

        private bool mCameraFollowWasEnabled;

        public int EnemyEntityId { get; private set; }

        public int PlayerEntityId { get; private set; }

        // 상대 포켓몬 번호를 받아 필드에 저장한 뒤 전투 포켓몬을 생성합니다.
        public void SpawnBattlePokemon(int enemyPokemonId)
        {
            if (enemyPokemonId <= 0)
            {
                Log.Error("BattlePokemonSpawner : 상대 포켓몬 번호는 1 이상이어야 합니다.");

                return;
            }

            mEnemyPokemonId = enemyPokemonId;

            SpawnBattlePokemon();
        }

        // 전투에 참가하는 포켓몬 두 마리를 생성합니다.
        public void SpawnBattlePokemon()
        {
            DespawnBattlePokemon();

            // Battle Scene 로드 이후 소환 함수까지 실행했는지 확인합니다.
            Log.Info($"[Battle Debug] 소환 시작 : 상대 ID : {mEnemyPokemonId}");

            mEntityComponent    = GameEntry.GetComponent<EntityComponent>();
            Camera battleCamera = Camera.main;

            if (mEntityComponent == null || battleCamera == null)
            {
                Log.Error("BattlePokemonSpawner : EntityComponent 또는 Main Camera가 없습니다.");

                return;
            }

            // 현재 플레이어 파티에서 첫 번째 포켓몬을 가져옵니다.
            PokemonData playerPokemon = GetPlayerPokemon();

            if (playerPokemon == null)
            {
                Log.Error("BattlePokemonSpawner : 전투에 참가할 아군 포켓몬이 없습니다.");

                return;
            }

            PokemonDatabase database = RPGGame.Instance.PokemonDatabase;
            if (database == null || !database.mIsLoaded)
            {
                Log.Warning("BattlePokemonSpawner : 포켓몬 JSON 로딩이 완료되지 않았습니다.");
                return;
            }

            PokemonInfo enemyInfo = database.GetPokemonInfo(mEnemyPokemonId);

            if (enemyInfo == null)
            {
                Log.Error($"BattlePokemonSpawner : JSON에서 상대 포켓몬 {mEnemyPokemonId} 정보를 찾을 수 없습니다.");
                return;
            }
            PokemonData enemyPokemon = new PokemonData(enemyInfo);

            // 포켓몬 번호에 해당하는 아군용 프리팹 주소를 조회합니다.
            string playerAssetName = GetPlayerPokemonAssetName(playerPokemon.PokemonId);

            if (string.IsNullOrEmpty(playerAssetName) == true)
            {
                Log.Error($"BattlePoekmonSpawner : " +
                    $"PokemonId = {playerPokemon.PokemonId}의" +
                    $"아군용 프리팹이 등록되지 않았습니다.");

                return;
            }

            // BattlePokemon Entity Group이 없다면 생성합니다.
            if (mEntityComponent.HasEntityGroup(mEntityGroupName) != true)
            {
                // Entity Group 생성 결과를 저장합니다.
                bool addResult = mEntityComponent.AddEntityGroup(mEntityGroupName, 60f, 16, 60f, 0);

                // 생성에 실패한 경우에만 종료합니다.
                if (addResult != true)
                {
                    Log.Error("BattlePokemonSpawner : BattlePokemon Entity Group 생성에 실패했습니다.");

                    return;
                }

                // Entity Group 생성 성공을 확인합니다.
                Log.Info("BattlePokemonSpawner : BattlePokemon Entity Group 생성 완료");
            }

            // 전투 중에는 필드 플레이어를 카메라가 따라가지 않도록 합니다.
            mCameraFollow = battleCamera.GetComponent<CameraFollow>();

            if (mCameraFollow != null)
            {
                mCameraFollowWasEnabled = mCameraFollow.enabled;
                mCameraFollow.enabled   = false;
            }

            // 상대 포켓몬과 아군 포켓몬의 생성 위치를 계산합니다.
            Vector3 enemyPosition   = GetSpawnPosition(battleCamera, mEnemySpawnPoint, mEnemyViewportPosition);
            Vector3 playerPosition  = GetSpawnPosition(battleCamera, mPlayerSpawnPoint, mPlayerViewportPosition);

            // 각 엔티티의 고유 ID를 발급합니다.
            PlayerEntityId = EntitySerialId.Next();
            EnemyEntityId  = EntitySerialId.Next();

            // 다른 상대 ID를 전달해도 현재 상대 프리팹은 이브이로 고정되어 있습니다.
            if (mEnemyPokemonId != 133)
            {
                Log.Warning($"[Battle Debug] 상대 ID = {mEnemyPokemonId}이지만 상대 프리팹은 {mEnemyPokemonAssetName}입니다.");
            }

            // 필드에 저장한 번호를 상대 포켓몬의 생성 데이터에 전달합니다.
            SpawnPokemon(new BattlePokemonData(EnemyEntityId, enemyPokemon.PokemonId, enemyPosition, null, true, mPokemonScale, enemyPokemon), enemyPosition, mEnemyPokemonAssetName);

            // 아군 포켓몬은 고정 ID 대신 파티의 포켓몬 ID를 사용합니다.
            SpawnPokemon(new BattlePokemonData(PlayerEntityId, playerPokemon.PokemonId, playerPosition, null, false, mPokemonScale, playerPokemon), playerPosition, playerAssetName);

            // 실제 출전하는 아군 포켓몬의 정보를 출력합니다.
            Log.Info(
                $"BattlePokemonSpawner : " +
                $"아군 = {playerPokemon.Name}," +
                $"PokemonId = {playerPokemon.PokemonId}, " +
                $"Asset = {playerAssetName}");
        }

        // 플레이어가 소지한 첫 번째 포켓몬을 가져옵니다.
        private PokemonData GetPlayerPokemon()
        {
            if (RPGGame.Instance == null)
            {
                Log.Error("BattlePokemonSpawner : RPGGame.Instance가 없습니다.");

                return null;
            }

            PokemonPartyManager partyManager = RPGGame.Instance.PokemonPartyManager;

            if (partyManager == null)
            {
                Log.Error("BattlePokemonSpawner : PokemonPartyManager가 없습니다.");

                return null;
            }

            if (partyManager.Count == 0)
            {
                Log.Warning("BattlePokemonSpawner : 소지 포켓몬이 없습니다.");

                return null;
            }

            // 현재는 파티의 첫 번째 포켓몬을 출전시킵니다.
            PokemonData pokemonData = partyManager.GetPokemon(0);

            return pokemonData;
        }

        // 포켓몬 번호에 해당하는 아군용 프리팹 주소를 가져옵니다.
        private string GetPlayerPokemonAssetName(int pokemonId)
        {
            // Inspector에 등록한 프리팹 주소를 우선 조회합니다.
            if (mPlayerPokemonPrefabs != null)
            {
                for (int index = 0; index < mPlayerPokemonPrefabs.Length; index++)
                {
                    PokemonPrefabMapping mapping = mPlayerPokemonPrefabs[index];

                    if (mapping == null)
                    {
                        continue;
                    }

                    if (mapping.PokemonId == pokemonId)
                    {
                        return mapping.AssetName;
                    }
                }
            }

            // 기존 피카츄 프리팹 연결은 그대로 사용합니다.
            if (pokemonId == 25)
            {
                return mPlayerPokemonAssetName;
            }

            // 등록되지 않은 포켓몬은 다른 프리팹으로 대체하지 않습니다.
            return null;
        }

        private Vector3 GetSpawnPosition(Camera camera, Transform spawnPoint, Vector2 viewportPosition)
        {
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }

            return camera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, mCameraDistance));
        }

        // 포켓몬 데이터와 프리팹 주소를 각각 전달받습니다.
        private void SpawnPokemon(BattlePokemonData pokemonData, Vector3 spawnPosition, string assetName)
        {
            if (pokemonData == null)
            {
                Log.Error("BattlePokemonData is null.");

                return;
            }

            if (string.IsNullOrEmpty(assetName) == true)
            {
                Log.Error("Battle Pokemon의 Addressable 이름이 비어 있습니다.");

                return;
            }

            pokemonData.Position = spawnPosition;

            // 로딩 중에도 종료 요청으로 취소할 수 있도록 먼저 ID를 기록합니다.
            mEntityIds.Add(pokemonData.Id);

            // [추가] 프리팹 로드 '요청'과 로드 성공을 구분하기 위한 로그입니다.
            Log.Info($"[Battle Debug] ShowEntity 요청 : EntityId = {pokemonData.Id}, PokemonId = {pokemonData.TypeId}, Asset = {assetName}, Position = {spawnPosition}");


            // UGF EntityComponent에서 포켓몬 프리팹을 생성합니다.
            mEntityComponent.ShowEntity(pokemonData.Id, typeof(BattlePokemon), assetName, mEntityGroupName, pokemonData);
        }

        // 전투 포켓몬 엔티티를 제거합니다.
        public void DespawnBattlePokemon()
        {
            if (mEntityComponent != null)
            {
                foreach (int entityId in mEntityIds)
                {
                    if (mEntityComponent.HasEntity(entityId) == true || mEntityComponent.IsLoadingEntity(entityId) == true)
                    {
                        mEntityComponent.HideEntity(entityId);
                    }
                }
            }

            mEntityIds.Clear();

            EnemyEntityId   = 0;
            PlayerEntityId  = 0;

            // 필드에서 사용하던 카메라 추적 상태를 복원합니다.
            if (mCameraFollow != null)
            {
                mCameraFollow.enabled   = mCameraFollowWasEnabled;
                mCameraFollow           = null;
            }
        }
    }
}