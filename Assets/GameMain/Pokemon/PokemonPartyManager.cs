using System.Collections.Generic;

namespace Monster
{
    public class PokemonPartyManager
    {
        // 소지할 수 있는 최대 포켓몬 수입니다.
        public const int MaxPartyCount = 6;

        // 현재 소지 중인 포켓몬 목록입니다.
        private List<PokemonData> mPokemonParty = new List<PokemonData>();

        // 현재 소지한 포켓몬 수입니다.
        public int Count { get { return mPokemonParty.Count; } }
        
        // 소지 포켓몬 목록에 포켓몬을 반환합니다.
        public bool AddPokemon(PokemonData pokemonData)
        {
            if (pokemonData == null)
            {
                return false;
            }

            // 이미 6마리를 소지하고 있다면 추가하지 않습니다.
            if (mPokemonParty.Count >= MaxPartyCount)
            {
                return false;
            }

            mPokemonParty.Add(pokemonData);

            return true;
        }

        // 지정한 위치의 포켓몬을 가져옵니다.
        public PokemonData GetPokemon(int index)
        {
            if (index < 0 || index >= mPokemonParty.Count)
            {
                return null;
            }

            return mPokemonParty[index];
        }

        // 지정한 위치의 포켓몬을 제거합니다.
        public bool RemovePokemon(int index)
        {
            if (index < 0 || index >= mPokemonParty.Count)
            {
                return false;
            }

            mPokemonParty.RemoveAt(index);

            return true;
        }
    }
}