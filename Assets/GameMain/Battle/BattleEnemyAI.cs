using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    // 상대 포켓몬이 자신의 턴에 사용할 공격 기술을 고르고 피해를 적용합니다.
    public sealed class BattleEnemyAI : GameFrameworkComponent
    {
        // <summary>
        // PP가 남은 공격 기술을 무작위로 하나 사용합니다.
        // 기술을 사용했다면 명중 여부와 실제 감소한 체력을 반환합니다.
        // </summary>
        public bool TryAttack(PokemonData attacker, PokemonData defender, out PokemonSkillInfo selectedSkill, out bool isHit, out int damage)
        {
            selectedSkill   = null;
            isHit           = false;
            damage          = 0;

            // 공격자나 방어자가 없거나 쓰러진 경우에는 기술을 사용하지 않습니다.
            if (attacker == null || defender == null || attacker.CurrentHP <= 0 || defender.CurrentHP <= 0)
            {
                return false;
            }

            // 보유 기술 중 실제 피해를 줄 수 있고 PP가 남은 기술만 고릅니다.
            List<PokemonSkillInfo> availableSkills = new List<PokemonSkillInfo>();

            foreach (PokemonSkillInfo skill in attacker.Skills)
            {
                if (skill != null && skill.Power > 0 && skill.CurrentPP > 0)
                {
                    availableSkills.Add(skill);
                }
            }

            if (availableSkills.Count == 0)
            {
                return false;
            }

            // GameFramework에 등록된 계산기를 찾은 뒤에만 기술을 사용합니다.
            BattleDamageCalculator calculator = GameEntry.GetComponent<BattleDamageCalculator>();

            if (calculator == null)
            {
                Log.Error("BattleEnemyAI : BattleDamageCalculator를 찾을 수 없습니다.");

                return false;
            }

            int selectedIndex = Random.Range(0, availableSkills.Count);
            selectedSkill = availableSkills[selectedIndex];

            // 명중하지 못하더라도 기술을 사용했으므로 PP는 한 번 차감합니다.
            selectedSkill.CurrentPP--;

            isHit = calculator.IsHit(selectedSkill.Accuracy);

            if (!isHit)
            {
                return true;
            }

            // 상성을 포함한 피해를 적용하고, 남은 체력에서 실제로 줄어든 양을 반환합니다.
            int previousHP          = defender.CurrentHP;
            int calculatedDamage    = calculator.CalculateDamage(attacker, defender, selectedSkill, out _);
            defender.SetHP(previousHP - calculatedDamage);

            damage                  = previousHP - defender.CurrentHP;

            return true;
        }
    }
}