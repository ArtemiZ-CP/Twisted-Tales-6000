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

                HeroEffects.GlobalEffect poisonGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 20,
                    Duration = 3,
                    Size = 1
                };

                HeroEffects.GlobalEffect healGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 30,
                    Duration = 3,
                    Size = 1
                };

                HeroEffects.GlobalEffect[] globalEffects = new HeroEffects.GlobalEffect[2];
                globalEffects[0] = poisonGlobalEffect;
                globalEffects[1] = healGlobalEffect;

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, globalEffects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastLevel2(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 120;

                HeroEffects.GlobalEffect poisonGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 30,
                    Duration = 3,
                    Size = 1
                };

                HeroEffects.GlobalEffect healGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 45,
                    Duration = 3,
                    Size = 1
                };

                HeroEffects.GlobalEffect[] globalEffects = new HeroEffects.GlobalEffect[2];
                globalEffects[0] = poisonGlobalEffect;
                globalEffects[1] = healGlobalEffect;

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, globalEffects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastLevel3(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 180;

                HeroEffects.GlobalEffect poisonGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 45,
                    Duration = 3,
                    Size = 1
                };

                HeroEffects.GlobalEffect healGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = 65,
                    Duration = 3,
                    Size = 1
                };

                HeroEffects.GlobalEffect[] globalEffects = new HeroEffects.GlobalEffect[2];
                globalEffects[0] = poisonGlobalEffect;
                globalEffects[1] = healGlobalEffect;

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, globalEffects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}