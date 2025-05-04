using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static unsafe class KingArthurAbilities
    {
        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int heroLevel = fightingHero.Hero.Level;
            int secondHeroAbilityIndex = HeroAbility.GetSecondHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);
            int thirdHeroAbilityIndex = HeroAbility.GetThirdHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);

            if (heroLevel == 0)
            {
                return TryCast(f, fightingHero, board, 140, 1 - FP._0_20);
            }
            else if (heroLevel == 1)
            {
                return TryCast(f, fightingHero, board, 210, 1 - FP._0_25);
            }
            else if (heroLevel == 2)
            {
                return TryCast(f, fightingHero, board, 315, 1 - FP._0_20 + FP._0_10);
            }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP damage, FP reduceAttackSpeed)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                List<FightingHero> heroesList = HeroBoard.GetAllTeamHeroesInHorizontalRange(f, target.Index, target.TeamNumber, board, 8, includeSelf: true);
                QList<FightingHero> fightingHeroes = f.ResolveList(board.FightingHeroesMap);
                Vector2Int fightingHeroCords = HeroBoard.GetHeroCords(fightingHero);

                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.IncreaseAttackSpeed,
                    Duration = 3,
                    Value = reduceAttackSpeed,
                };

                foreach (FightingHero hero in heroesList)
                {
                    HeroAttack.DamageHero(f, fightingHero, board, hero, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);

                    Vector2Int heroCords = HeroBoard.GetHeroCords(hero);
                    Vector2Int newHeroCords = new(heroCords.x, heroCords.y + (fightingHeroCords.y <= heroCords.y ? 1 : -1));

                    if (HeroBoard.TryConvertCordsToIndex(newHeroCords, out int index))
                    {
                        FightingHero nextHero = fightingHeroes[index];
                        HeroAttack.DamageHero(f, fightingHero, board, nextHero, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                    }
                }

                return true;
            }

            return false;
        }
    }
}