namespace Monster
{
    // 포켓몬의 속성(타입)을 정의합니다.
    public enum EPokemonType
    {
        None = 0,
        Normal,
        Fire,
        Water,
        Electric,
        Grass,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dragon
    }

    // 포켓몬의 기본 정보 및 능력치를 관리합니다.
    public class  PokemonData
    {
        // 포켓몬의 도감 번호입니다.
        public int PokemonId    { get; private set; }

        // 포켓몬의 이름입니다.
        public string Name      { get; private set; }

        // 포켓몬의 현재 레벨입니다.
        public int Level        { get; private set; }

        // 포켓몬의 첫 번째 타입입니다.
        public EPokemonType PrimaryType     { get; private set; }
        
        // 단일 타입 포켓몬은 None을 사용합니다.
        public EPokemonType SecondaryType   { get; private set; }

        // 포켓몬의 체력입니다.
        public int MaxHP        { get; private set; }
        public int CurrentHP    { get; private set; }

        // 포켓몬의 공격력입니다.
        public int Attack       { get; private set; }
        public int Action       { get; private set; }

        public int Defense      { get; private set; }
        public int SpAttack     { get; private set; }
        public int SpDefense    { get; private set; }
        public int Speed        { get; private set; }

        // 포켓몬 기본 데이터를 초기화합니다.
        public PokemonData(PokemonInfo pokemonInfo)
        {
            if (pokemonInfo == null)
            {
                throw new System.ArgumentNullException(nameof(pokemonInfo));
            }

            PokemonId   = pokemonInfo.PokemonId;
            Name        = pokemonInfo.Name;
            Level       = pokemonInfo.Level;

            // 포켓몬 타입을 문자열에서 enum으로 변환합니다.
            EPokemonType primaryType;
            EPokemonType secondaryType;

            if (System.Enum.TryParse(pokemonInfo.PrimaryType, true, out primaryType) == true)
            {
                PrimaryType = primaryType;
            }
            else
            {
                PrimaryType = EPokemonType.None;
            }

            if (System.Enum.TryParse(pokemonInfo.SecondaryType, true, out secondaryType) == true)
            {
                SecondaryType = secondaryType;
            }
            else
            {
                SecondaryType = EPokemonType.None;
            }

            MaxHP       = System.Math.Max(0, pokemonInfo.MaxHP);
            CurrentHP   = System.Math.Clamp(pokemonInfo.CurrentHP, 0, MaxHP);

            Attack      = pokemonInfo.Attack;
            Action      = pokemonInfo.Action;
            Defense     = pokemonInfo.Defense;
            SpAttack    = pokemonInfo.SpAttack;
            SpDefense   = pokemonInfo.SpDefense;
            Speed       = pokemonInfo.Speed;
        }

        // 포켓몬의 현재 체력을 변경합니다.
        public void SetHP(int hp)
        {
            CurrentHP = System.Math.Clamp(hp, 0, MaxHP);
        }
    }
}