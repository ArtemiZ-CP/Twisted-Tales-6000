using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static unsafe class HatterAbilities
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
                return TryCastLevel1(f, fightingHero, board);
            }
            else if (heroLevel == 1)
            {
                return TryCastLevel2(f, fightingHero, board);
            }
            else if (heroLevel == 2)
            {
                return TryCastLevel3(f, fightingHero, board);
            }

            return false;
        }

        private static bool TryCastLevel1(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 80;
                FP heal = 30;
                HeroAbility.HealAllAllies(f, fightingHero, board, heal);

                HeroEffects.GlobalEffect globalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 20,
                    Duration = 3,
                    Size = 1
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, globalEffect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastLevel2(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 120;
                FP heal = 45;
                HeroAbility.HealAllAllies(f, fightingHero, board, heal);

                HeroEffects.GlobalEffect globalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 30,
                    Duration = 3,
                    Size = 1
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, globalEffect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastLevel3(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 180;
                FP heal = 65;
                HeroAbility.HealAllAllies(f, fightingHero, board, heal);

                HeroEffects.GlobalEffect globalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 45,
                    Duration = 3,
                    Size = 1
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, globalEffect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}