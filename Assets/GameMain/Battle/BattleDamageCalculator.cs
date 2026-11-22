using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monster
{
    public class BattleDamageCalculator
    {
        private static readonly Dictionary<string, float> mTypeChart =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        static BattleDamageCalculator()
        {
            Add("Normal", "Rock Steal", 0.5f);
            Add("Normal", "Ghost", 0f);

            Add("Fire", "Grass Ice Bug Steel", 2f);
            Add("Fire", "Fire Water Rock Dragon", 0.5f);

            Add("Water", "Fire Ground Rock", 2f);
            Add("Water", "Water Grass Dragon", 0.5f);

            Add("Electric", "Water Flying", 2f);
            Add("Electric", "Electric Grass Dragon", 0.5f);
            Add("Electric", "Ground", 0f);

            Add("Grass", "Water Ground Rock", 2f);
            Add("Grass", "Fire Grass Poison Flying Bug Dragon Steel", 0.5f);

            Add("Ice", "Grass Ground Flying Dragon", 2f);
            Add("Ice", "Fire Water Ice Steel", 0.5f);

            Add("Fighting", "Normal Ice Rock Dark Steel", 2f);
            Add("Fighting", "Poison Flying Psychic Bug Fairy", 0.5f);
            Add("Fighting", "Ghost", 0f);

            Add("Poison", "Grass Fairy", 2f);
            Add("Poison", "Poison Ground Rock Ghost", 0.5f);
            Add("Poison", "Steel", 0f);

            Add("Flying", "Grass Fighting Bug", 2f);
            Add("Flying", "Psychic Steel", 0.5f);
            Add("Psychic", "Dark", 0f);

            Add("Psychic", "Fighting Poison", 2f);
            Add("Psychic", "Psychic Steel", 0.5f);
            Add("Psychic", "Dark", 0f);

            Add("Bug", "Grass Psychic Dark", 2f);
            Add("Bug", "Fire Fighting Poison Flying Ghost Steel Fairy", 0.5f);

            Add("Rock", "Fire Ice Flying Bug", 2f);
            Add("Rock", "Fighting Ground Steel", 0.5f);

            Add("Ghost", "Psychic Ghost", 2f);
            Add("Ghost", "Dark", 0.5f);
            Add("Ghost", "Normal", 0f);

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

        private static void Add(string attackType, string defenseTypes, float multiplier)
        {
            string[] types = defenseTypes.Split(' ');

            for (int index = 0; index < types.Length; index++)
            {
                mTypeChart[attackType + ":" + types[index]] = multiplier;
            }
        }

        public static bool IsHit(int accuracy)
        {
            return UnityEngine.Random.Range(0, 100) < Mathf.Clamp(accuracy, 0, 100);
        }

        public static float GetTypeMultiplier(string attackType, PokemonData defender)
        {
            if (defender == null || string.IsNullOrEmpty(attackType) == true)
            {
                return 1f;
            }

            float multiplier = GetSingleTypeMultiplier(attackType, defender.PrimaryType);

            if (defender.SecondaryType != EPokemonType.None && defender.SecondaryType != defender.PrimaryType)
            {
                multiplier *= GetSingleTypeMultiplier(attackType, defender.SecondaryType);
            }

            return multiplier;
        }

        private static float GetSingleTypeMultiplier(string attackType, EPokemonType defenseType)
        {
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

            return 1f;
        }

        public static int CalculateDamage(PokemonData attacker, PokemonData defender, PokemonSkillInfo skill, out float typeMultiplier)
        {
            typeMultiplier = GetTypeMultiplier(skill.Type, defender);

            if (skill.Power <= 0 || typeMultiplier <= 0f)
            {
                return 0;
            }

            bool isSpecial = string.Equals(skill.Category, "Special", StringComparison.OrdinalIgnoreCase);

            float attack = isSpecial == true ? attacker.SpAttack : attacker.Attack;
            float defense = isSpecial == true ? defender.SpDefense : defender.Defense;
            float levelFactor = Mathf.Floor(attacker.Level * 2f / 5f + 2f);
            float baseDamage = Mathf.Floor(levelFactor * skill.Power * Mathf.Max(1f, attack) / Mathf.Max(1f, defense) / 50f) + 2f;

            bool sameType = string.Equals(skill.Type, attacker.PrimaryType.ToString(), StringComparison.OrdinalIgnoreCase) == true
                || (attacker.SecondaryType != EPokemonType.None && string.Equals(skill.Type, attacker.SecondaryType.ToString(), StringComparison.OrdinalIgnoreCase) == true);

            float stab = sameType == true ? 1.5f : 1f;

            float randomMultiplier = UnityEngine.Random.Range(85, 101) / 100f;

            return Mathf.Max(1, Mathf.FloorToInt(baseDamage * stab * typeMultiplier * randomMultiplier));
        }
    }
}