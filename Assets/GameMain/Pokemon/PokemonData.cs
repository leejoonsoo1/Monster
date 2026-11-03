namespace Monster
{
    // 포켓몬의 속성(타입)을 정의합니다.
    public enum EPokemonType
    {
        None = 0,
        Normal = 1,
        Fire = 2,
        Water = 3,
        Electric = 4,
        Grass = 5,
        Ice = 6,
        Fighting = 7,
        Poison = 8,
        Ground = 9,
        Flying = 10,
        Psychic = 11,
        Bug = 12,
        Rock = 13,
        Ghost = 14,
        Dragon = 15
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
        public int Defense      { get; private set; }
        public int SpAttack     { get; private set; }
        public int SpDefense    { get; private set; }
        public int Speed        { get; private set; }

        // 포켓몬 기본 데이터를 초기화합니다.
        public PokemonData(int pokemonId, string name, int level, EPokemonType primaryType, EPokemonType secondaryType, int maxHP, int attack, int defense, int specialAttack, int specialDefense, int speed)
        {
            PrimaryType     = primaryType;
            SecondaryType   = secondaryType;

            MaxHP           = maxHP;
            CurrentHP       = maxHP;

            Attack          = attack;
            Defense         = defense;
            SpAttack        = specialAttack;
            SpDefense       = specialDefense;
            Speed           = speed;
        }

        // 포켓몬의 현재 체력을 변경합니다.
        public void SetHP(int hp)
        {
            CurrentHP = System.Math.Clamp(hp, 0, MaxHP);
        }
    }
}