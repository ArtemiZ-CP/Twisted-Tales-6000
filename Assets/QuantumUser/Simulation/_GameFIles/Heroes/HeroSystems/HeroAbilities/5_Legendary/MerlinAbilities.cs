using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class MerlinAbilities
    {
        public static bool TryGetNewAttackAction(ref FightingHero fightingHero, QList<FightingHero> heroes, out Func<Frame, FightingHero, HeroAttack.DamageType, HeroAttack.AttackType, bool> attackAction)
        {
            if ((fightingHero.AttackStage %= 3) == 2)
            {
                attackAction = Attack;
                return true;
            }

            attackAction = null;
            return false;
        }

        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int heroLevel = fightingHero.Hero.Level;
            int secondHeroAbilityIndex = HeroAbility.GetSecondHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);
            int thirdHeroAbilityIndex = HeroAbility.GetThirdHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);

            FP damage = heroLevel switch
            {
                0 => 140,
                1 => 210,
                2 => 315,
                _ => 0
            };

            return TryCastMain(f, fightingHero, board, damage);
        }

        private static bool TryCastMain(Frame f, FightingHero fightingHero, Board board, FP damage)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool Attack(Frame f, FightingHero fightingHero, HeroAttack.DamageType damageType, HeroAttack.AttackType attackType)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);

            if (HeroAttack.IsAbleToAttack(f, fightingHero, board, out FightingHero targetHero) == false)
            {
                return false;
            }

            int heroLevel = fightingHero.Hero.Level;
            FP heroDamage = fightingHero.Hero.AttackDamage * 2;

            switch (f.RNG->Next(0, 3))
            {
                case 0:
                    FP silenceDuration = heroLevel switch
                    {
                        0 => FP._1_50,
                        1 => 2,
                        2 => 3,
                        _ => 0
                    };
                    HeroEffects.Effect effect0 = new()
                    {
                        Owner = fightingHero.Hero.Ref,
                        Type = HeroEffects.EffectType.BlastSilence,
                        Duration = silenceDuration,
                        Size = 2,
                    };
                    HeroAttack.ProjectileAttack(f, fightingHero, board, targetHero, heroDamage, effect0, damageType, attackType);
                    break;
                case 1:
                    FP healPercent = heroLevel switch
                    {
                        0 => FP._0_50 - FP._0_05,
                        1 => FP._0_75 - FP._0_10,
                        2 => FP._0_75 + FP._0_05,
                        _ => 0
                    };
                    int heal = (int)(fightingHero.Hero.AttackDamage * healPercent);
                    List<FightingHero> alies = HeroBoard.GetAllAliesInRange(f, fightingHero, board, includeSelf: true);
                    FightingHero alyWithMinHealth = HeroBoard.GetAliyWithMinHealth(alies);
                    HeroAttack.HealHero(f, fightingHero, board, alyWithMinHealth, heal);
                    HeroAttack.ProjectileAttack(f, fightingHero, board, targetHero, heroDamage, damageType, attackType);
                    break;
                case 2:
                    FP damagePercent = heroLevel switch
                    {
                        0 => FP._0_20 * 2,
                        1 => FP._0_50 + FP._0_10,
                        2 => FP._0_20 * 4,
                        _ => 0
                    };
                    FP damage = (int)(heroDamage * damagePercent);
                    HeroEffects.Effect effect2 = new()
                    {
                        Owner = fightingHero.Hero.Ref,
                        Type = HeroEffects.EffectType.Blast,
                        Value = damage,
                        Size = 1,
                    };
                    HeroAttack.ProjectileAttack(f, fightingHero, board, targetHero, heroDamage, effect2, damageType, attackType);
                    break;
            }

            return true;
        }
    }
}