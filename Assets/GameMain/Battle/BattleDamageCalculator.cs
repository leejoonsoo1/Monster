using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    // <summary>
    // GameFramework에 등록되어 기술의 명중률, 타입 상성, 피해량을 계산합니다.
    // </summary>
    public sealed class BattleDamageCalculator : GameFrameworkComponent
    {
        // "공격 타입:방어 타입"을 키로 사용하며 타입 이름의 대소문자를 구분하지 않습니다.
        private static readonly Dictionary<string, float> mTypeChart =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        // 기본 배율인 1배와 다른 조합을 공격 타입별로 등록합니다.
        static BattleDamageCalculator()
        {
            // 노말 타입 공격입니다.
            Add("Normal", "Rock Steal", 0.5f);
            Add("Normal", "Ghost", 0f);

            // 불꽃, 물, 전기 타입 공격입니다.
            Add("Fire", "Grass Ice Bug Steel", 2f);
            Add("Fire", "Fire Water Rock Dragon", 0.5f);

            Add("Water", "Fire Ground Rock", 2f);
            Add("Water", "Water Grass Dragon", 0.5f);

            Add("Electric", "Water Flying", 2f);
            Add("Electric", "Electric Grass Dragon", 0.5f);
            Add("Electric", "Ground", 0f);

            // 풀, 얼음, 격투 타입 공격입니다.
            Add("Grass", "Water Ground Rock", 2f);
            Add("Grass", "Fire Grass Poison Flying Bug Dragon Steel", 0.5f);

            Add("Ice", "Grass Ground Flying Dragon", 2f);
            Add("Ice", "Fire Water Ice Steel", 0.5f);

            Add("Fighting", "Normal Ice Rock Dark Steel", 2f);
            Add("Fighting", "Poison Flying Psychic Bug Fairy", 0.5f);
            Add("Fighting", "Ghost", 0f);

            // 독, 비행, 에스퍼 타입 공격입니다.
            Add("Poison", "Grass Fairy", 2f);
            Add("Poison", "Poison Ground Rock Ghost", 0.5f);
            Add("Poison", "Steel", 0f);

            Add("Flying", "Grass Fighting Bug", 2f);
            Add("Flying", "Psychic Steel", 0.5f);
            Add("Psychic", "Dark", 0f);

            Add("Psychic", "Fighting Poison", 2f);
            Add("Psychic", "Psychic Steel", 0.5f);
            Add("Psychic", "Dark", 0f);

            // 벌레, 바위, 고스트 타입 공격입니다.
            Add("Bug", "Grass Psychic Dark", 2f);
            Add("Bug", "Fire Fighting Poison Flying Ghost Steel Fairy", 0.5f);

            Add("Rock", "Fire Ice Flying Bug", 2f);
            Add("Rock", "Fighting Ground Steel", 0.5f);

            Add("Ghost", "Psychic Ghost", 2f);
            Add("Ghost", "Dark", 0.5f);
            Add("Ghost", "Normal", 0f);

            // 드래곤, 악, 강철, 페어리 타입 공격입니다.
            Add("Dragon", "Dragon", 2f);
            Add("Dragon", "Steel", 0.5f);
            Add("Dragon", "Fairy", 0f);

            Add("Dark", "Psychic Ghost", 2f);
            Add("Dark", "Fighting Dark Fairy", 0.5f);

            Add("Steel", "Psychic Ghost", 2f);
            Add("Steel", "Fire Water Electric Steel", 0.5f);

            Add("Fairy", "Fighting Dragon Dark", 2f);
            Add("Fairy", "Fire Poison Steel", 0.5f);
        }

        // <summary>
        // 하나의 공격 타입과 여러 방어 타입에 같은 상성 배율을 등록합니다.
        // </summary>
        private static void Add(string attackType, string defenseTypes, float multiplier)
        {
            // 공백으로 구분된 방어 타입을 각각 등록합니다.
            string[] types = defenseTypes.Split(' ');

            for (int index = 0; index < types.Length; index++)
            {
                mTypeChart[attackType + ":" + types[index]] = multiplier;
            }
        }

        // <summary>
        // 기술 정확도를 0~100으로 제한하고, 0~99 난수로 명중 여부를 판정합니다.
        // </summary>
        public bool IsHit(int accuracy)
        {
            return UnityEngine.Random.Range(0, 100) < Mathf.Clamp(accuracy, 0, 100);
        }

        // <summary>
        // 공격 기술과 방어 포켓몬의 타입을 비교하여 최종 상성 배율을 계산합니다.
        // </summary>
        public float GetTypeMultiplier(string attackType, PokemonData defender)
        {
            // 방어 포켓몬이나 기술 타입 정보가 없으면 기본 배율을 사용합니다.
            if (defender == null || string.IsNullOrEmpty(attackType) == true)
            {
                return 1f;
            }

            // 첫 번째 방어 타입의 배율을 구합니다.
            float multiplier = GetSingleTypeMultiplier(attackType, defender.PrimaryType);

            // 서로 다른 두 번째 방어 타입이 있으면 두 타입의 배율을 곱합니다.
            if (defender.SecondaryType != EPokemonType.None && defender.SecondaryType != defender.PrimaryType)
            {
                multiplier *= GetSingleTypeMultiplier(attackType, defender.SecondaryType);
            }

            return multiplier;
        }

        // 공격 타입과 단일 방어 타입의 상성을 조회합니다.
        private static float GetSingleTypeMultiplier(string attackType, EPokemonType defenseType)
        {
            // 방어 타입이 없으면 상성 보정을 적용하지 않습니다.
            if (defenseType == EPokemonType.None)
            {
                return 1f;
            }

            float multiplier;
            string key = attackType + ":" + defenseType;

            if (mTypeChart.TryGetValue(key, out multiplier) == true)
            {
                return multiplier;
            }

            // 테이블에 없는 조합은 기본 배율인 1배입니다.
            return 1f;
        }

        // <summary>
        // 포켓몬의 능력치와 기술 데이터를 바탕으로 피해량을 계산하고 상성 배율도 반환합니다.
        // </summary>
        public int CalculateDamage(PokemonData attacker, PokemonData defender, PokemonSkillInfo skill, out float typeMultiplier)
        {
            typeMultiplier = GetTypeMultiplier(skill.Type, defender);

            // 위력이 없는 기술이나 상대가 면역인 기술은 피해를 주지 않습니다.
            if (skill.Power <= 0 || typeMultiplier <= 0f)
            {
                return 0;
            }

            // 특수 기술은 특수공격과 특수방어를 사용하고, 그 외에는 공격과 방어를 사용합니다.
            bool isSpecial = string.Equals(skill.Category, "Special", StringComparison.OrdinalIgnoreCase);

            float attack;
            float defense;

            if (isSpecial)
            {
                attack  = attacker.SpAttack;
                defense = defender.SpDefense;
            }
            else
            {
                attack  = attacker.Attack;
                defense = defender.Defense;
            }

            // 레벨, 기술 위력, 공격 및 방어 능력치로 기본 피해량을 계산합니다.
            // 능력치의 최솟값을 1로 적용하여 방어 값이 0이어도 나눌 수 있게 합니다.
            float levelFactor   = Mathf.Floor(attacker.Level * 2f / 5f + 2f);
            float baseDamage    = Mathf.Floor(levelFactor * skill.Power * Mathf.Max(1f, attack) / Mathf.Max(1f, defense) / 50f) + 2f;

            // 기술이 공격 포켓몬의 타입과 일치하는지 확인합니다.
            bool sameType = false;
            
            if (string.Equals(skill.Type, attacker.PrimaryType.ToString(), StringComparison.OrdinalIgnoreCase) == true )
            {
                sameType = true;
            }
            else if (attacker.SecondaryType != EPokemonType.None)
            {
                if (string.Equals(skill.Type, attacker.SecondaryType.ToString(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    sameType = true;
                }
            }
            
            // 자속 보너스를 적용합니다. 타입이 일치하면 1.5배입니다.
            float stab;

            if (sameType)
            {
                stab = 1.5f;
            }
            else
            {
                stab = 1f;
            }

            // 피해량에 85%~100%의 난수 보정을 적용합니다.
            float randomMultiplier = UnityEngine.Random.Range(85, 101) / 100f;

            // 보정을 모두 적용한 뒤 내림하고, 최소 피해량을 1로 보장합니다.
            return Mathf.Max(1, Mathf.FloorToInt(baseDamage * stab * typeMultiplier * randomMultiplier));
        }
    }
}