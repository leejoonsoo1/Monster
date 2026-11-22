using System;
using System.Collections.Generic;

namespace Monster
{
    // 기술 타입별 커서 스프라이트 이름을 관리합니다.
    public static class PokemonTypeCursorTable
    {
        // Key : JSON의 기술 타입 문자열
        // Value : (비활성화 스프라이트 이름, 활성화 스프라이트 이름)
        private static readonly Dictionary<string, (string Inactive, string Active)> mCursorTable =
            new Dictionary<string, (string Inactive, string Active)>(StringComparer.OrdinalIgnoreCase)
            {
                {"Normal",   ("cursor_fight_0", "cursor_fight_1"   ) },
                {"Fire",     ("cursor_fight_2", "cursor_fight_3"   ) },
                {"Water",    ("cursor_fight_4", "cursor_fight_5"   ) },
                {"Ghost",    ("cursor_fight_6", "cursor_fight_7"   ) },
                {"Ground",   ("cursor_fight_8", "cursor_fight_9"   ) },
                {"Rock",     ("cursor_fight_10", "cursor_fight_11" ) },
                {"Electric", ("cursor_fight_12", "cursor_fight_13" ) },
                {"Fairy",    ("cursor_fight_14", "cursor_fight_15" ) },
                {"Steel",    ("cursor_fight_16", "cursor_fight_17" ) },
                {"Psychic",  ("cursor_fight_18", "cursor_fight_19" ) },
                {"Fighting", ("cursor_fight_20", "cursor_fight_21" ) },
                {"Ice",      ("cursor_fight_22", "cursor_fight_23" ) },
                {"Grass",    ("cursor_fight_24", "cursor_fight_25" ) },
                {"Bug",      ("cursor_fight_26", "cursor_fight_27" ) },
                {"Poison",   ("cursor_fight_28", "cursor_fight_29" ) },
                {"Flying",   ("cursor_fight_30", "cursor_fight_31" ) },
                {"Dragon",   ("cursor_fight_32", "cursor_fight_33" ) },
                {"Dark",     ("cursor_fight_34", "cursor_fight_35" ) }
            };

        // 타입이 테이블에 존재하는지 확인합니다.
        public static bool ContainsType(string type)
        {
            if (string.IsNullOrEmpty(type) == true)
            {
                return false;
            }

            return mCursorTable.ContainsKey(type);
        }

        // 타입에 해당하는 스프라이트 이름 2개를 반환합니다.
        public static bool TryGetSpriteNames(string type, out string inactiveSpriteName, out string activeSpriteName)
        {
            inactiveSpriteName  = null;
            activeSpriteName    = null;

            if (string.IsNullOrEmpty(type) == true)
            {
                return false;
            }

            (string Inactive, string Active) sprites;

            if (mCursorTable.TryGetValue(type, out sprites)!= true)
            {
                return false;
            }

            inactiveSpriteName  = sprites.Inactive;
            activeSpriteName    = sprites.Active;

            return true;
        }

        // 타입과 선택 여부에 맞는 스프라이트 이름을 반환합니다.
        public static string GetSpriteName(string type, bool isActive)
        {
            if (string.IsNullOrEmpty(type) == true)
            {
                return null;
            }

            (string Inactive, string Active) sprites;

            if (mCursorTable.TryGetValue(type, out sprites) != true)
            {
                return null;
            }

            if (isActive == true)
            {
                return sprites.Active;
            }


            return sprites.Inactive;
        }
    }
}
