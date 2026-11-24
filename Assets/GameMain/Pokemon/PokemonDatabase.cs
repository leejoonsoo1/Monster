using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityGameFramework.Runtime;

namespace Monster
{
    // JSON 파일에서 포켓몬 데이터를 읽어오는 클래스입니다.
    [Serializable]
    public class PokemonSkillInfo
    {
        public string   Name;
        public string   Type;
        public string   Category;

        public int      Power;
        public int      Accuracy;
        public int      MaxPP;
        public int      CurrentPP;
        public int      Priority;
    }

    // JSON 파일에서 포켓몬 데이터를 읽어오는 클래스입니다.
    [Serializable]
    public class PokemonInfo
    {
        public int      PokemonId;
        public string   Name;
        public int      Level;

        public string   PrimaryType;
        public string   SecondaryType;

        public int      MaxHP;
        public int      CurrentHP;

        public int      Attack;
        public int      Action;
        public int      Defense;
        public int      SpAttack;
        public int      SpDefense;
        public int      Speed;

        public int      Experience;
        public int      ExperienceToNextLevel;

        public PokemonSkillInfo[] Skills;

        // 포켓몬이 배울 수 있는 스킬 목록입니다.
        public List<PokemonSkillInfo> mSkills;
    }

    // JSON 최상위의 Pokemon 배열을 읽기 위한 클래스입니다.
    [Serializable]
    public class  PokemonBattleInfoJson
    {
        public PokemonInfo[] Pokemon;
    }

    // 포켓몬 JSON 데이터를 불러오고 조회하는 클래스입니다.
    public class  PokemonDatabase
    {
        // Addressables에 등록할 JSON의 주소입니다.
        private const string mAssetName = "Assets/GameMain/Pokemon/PokemonBattleInfo.json";

        // 도감 번호를 기준으로 포켓몬 정보를 보관합니다.
        private readonly Dictionary<int, PokemonInfo> mPokemonInfos = new Dictionary<int, PokemonInfo>();

        // 현재 JSON을 불러오는 중인지 확인합니다.
        private bool    mIsLoading = false;

        // 데이터베이스 초기화 완료 여부입니다.
        public bool     mIsLoaded { get; private set; }

        // JSON 로딩이 완료되면 알리는 이벤트입니다.
        public event Action Loaded;

        // 등록된 포켓몬 수를 반환합니다.
        public int Count
        {
            get { return mPokemonInfos.Count; }
        }

        public void Initialize()
        {
            // 이미 초기화했거나 로딩 중이면 중복 요청하지 않습니다.
            if (mIsLoaded == true || mIsLoading == true)
            {
                return;
            }

            mIsLoading = true;

            Log.Info("PokemonDatabase : JSON 로딩 시작");

            // Addressables를 이용해 JSON을 TextAsset으로 불러옵니다.
            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(mAssetName);

            // 완료 이벤트에 등록합니다.
            handle.Completed += OnLoadCompleted;
        }

        // JSON 파일의 비동기 로딩이 완료되면 호출합니다.
        private void OnLoadCompleted(AsyncOperationHandle<TextAsset> operation)
        {
            mIsLoading = false;

            try
            {
                // 로딩 성공 여부와 TextAsset 존재 여부를 확인합니다.
                if (operation.Status != AsyncOperationStatus.Succeeded || operation.Result == null)
                {
                    Log.Error($"PokemonDatabase : JSON 로딩 실패 - Asset = {mAssetName}, " +
                        $"Status = {operation.Status}, Error = {operation.OperationException}");

                    return;
                }

                // 로드한 TextAsset에서 JSON 문자열을 가져옵니다.
                string jsonText = operation.Result.text;

                // JSON 문자열을 C# 데이터 객체로 변환합니다.
                PokemonBattleInfoJson jsonData = JsonUtility.FromJson<PokemonBattleInfoJson>(jsonText);

                // JSON에 포켓몬 정보가 있는지 확인합니다.
                if (jsonData == null || jsonData.Pokemon == null || jsonData.Pokemon.Length == 0)
                {
                    Log.Error("PokemonDatabase : 포켓몬 데이터가 없습니다.");

                    return;
                }

                // 기존 데이터를 초기화합니다.
                mPokemonInfos.Clear();

                // JSON에서 등록된 포켓몬 정보를 순서대로 저장합니다.
                for (int index = 0; index < jsonData.Pokemon.Length; index++)
                {
                    PokemonInfo pokemonInfo = jsonData.Pokemon[index];

                    // 데이터가 비어 있으면 건너뜁니다.
                    if (pokemonInfo == null)
                    {
                        continue;
                    }

                    // 유효하지 않은 포켓몬 번호는 등록하지 않습니다.
                    if (pokemonInfo.PokemonId <= 0)
                    {
                        Log.Error("PokemonDatabase : 잘못된 PokemonId입니다.");

                        continue;
                    }

                    // 동일한 도감 번호가 이미 등록되어 있는지 확인합니다.
                    if (mPokemonInfos.ContainsKey(pokemonInfo.PokemonId) == true)
                    {
                        Log.Warning($"PokemonDatabase : 중복된 pokemonId {pokemonInfo.PokemonId}");

                        continue;
                    }

                    // 도감 번호를 기준으로 정보를 저장합니다.
                    mPokemonInfos.Add(pokemonInfo.PokemonId, pokemonInfo);
                    Log.Info($"PokemonDatabase : {pokemonInfo.PokemonId} - { pokemonInfo.Name}");
                }

                // 포켓몬 정보가 하나 이상 등록되었으면 초기화를 완료합니다.
                mIsLoaded = mPokemonInfos.Count > 0;

                Log.Info($"PokemonDatabase : 초기화 완료, 등록된 포켓몬 수 = {Count}");
            }
            catch(Exception exception)
            {
                mIsLoaded = false;
                mPokemonInfos.Clear();

                Log.Error($"PokemonDatabase :  JSON 처리 오류 - {exception}");
            }
            finally
            {
                // 완료 이벤트 등록을 해제합니다.
                operation.Completed -= OnLoadCompleted;

                // JSON 파싱이 끝났으므로 Addressables 핸들을 반환합니다.
                if (operation.IsValid() == true)
                {
                    Addressables.Release(operation);
                }
            }

            // 로딩이 성공하면 파티 매니저로 완료 사실을 알립니다.
            if (mIsLoaded == true)
            {
                if (Loaded != null)
                {
                    Loaded.Invoke();
                }
            }
        }

        // 도감 번호에 해당하는 포켓몬 기본 정보를 조회합니다.
        public PokemonInfo GetPokemonInfo(int pokemonId)
        {
            // 데이터베이스 로딩이 완료되지 않았다면 조회하지 않습니다.
            if (mIsLoaded != true)
            {
                Log.Warning("PokemonDatabase : 초기화가 완료되지 않았습니다.");

                return null;
            }

            PokemonInfo pokemonInfo;

            // 등록된 도감 번호를 검색합니다.
            if (mPokemonInfos.TryGetValue(pokemonId, out pokemonInfo) == true)
            {
                return pokemonInfo;
            }

            Log.Warning($"PokemonDatabase : PokemonId = {pokemonId}를 찾을 수 없습니다.");
            
            return null;
        }
    }
}