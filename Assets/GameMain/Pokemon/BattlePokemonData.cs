using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    // 전투 포켓몬 생성에 필요한 데이터를 관리합니다.
    public class BattlePokemonData : EntityData
    {
        // 표시할 포켓몬 Sprite입니다.
        public Sprite PokemonSprite { get; private set; }

        // 상대 포켓몬 여부입니다.
        public bool IsEnemy         { get; private set; }

        // 월드에 표시할 포켓몬의 배율입니다.
        public float Scale          { get; private set; }

        // JSON에서 읽은 전투 능력치입니다. 아군은 파티의 원본 데이터를 사용합니다.
        public PokemonData Stats { get; private set; }

        public BattlePokemonData(
            int id,
            int typeId,
            Vector3 position,
            Sprite pokemonSprite,
            bool isEnemy,
            float scale = 1f,
            PokemonData stats = null) : base(id, typeId)
        {
            Position        = position;
            PokemonSprite   = pokemonSprite;
            IsEnemy         = isEnemy;
            Scale           = scale;
            Stats           = stats;
        }
    }
}