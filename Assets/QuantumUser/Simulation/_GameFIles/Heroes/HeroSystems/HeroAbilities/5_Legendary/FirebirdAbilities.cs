using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class FirebirdAbilities
    {
        public const int Range = 2;

        public static void ProcessDeathInFirebirdRebirthRange(Frame f, ref FightingHero targetHero, Board board, QList<FightingHero> heroes)
        {
            // List<FightingHero> fightingHeroes = HeroBoard.GetAllTeamHeroesInRange(f, targetHero.Index, HeroBoard.GetEnemyTeamNumber(targetHero.TeamNumber), board, FirebirdAbilities.Range);

            // for (int i = 0; i < fightingHeroes.Count; i++)
            // {
            //     FightingHero hero = fightingHeroes[i];
            //     QList<EffectQnt> effects = f.ResolveList(hero.Effects);

            //     for (int j = 0; j < effects.Count; j++)
            //     {
            //         EffectQnt effectQnt = effects[j];

            //         if (effectQnt.Index == (int)HeroEffects.EffectType.FirebirdRebirth)
            //         {
            //             effectQnt.Size++;
            //         }

            //         effects[j] = effectQnt;
            //     }
            // }
        }

        public static void ProcessLoseLife(Frame f, ref FightingHero targetHero, Board board, QList<FightingHero> heroes)
        {
            // targetHero.ExtraLives--;
            // targetHero.CurrentHealth = 0;
            // heroes[targetHero.Index] = targetHero;

            // int duration = 4;
            // FP attackSpeedIncrease = targetHero.Hero.Level switch
            // {
            //     Hero.Level1 => FP._0_20,
            //     Hero.Level2 => FP._0_25,
            //     Hero.Level3 => FP._0_25 + FP._0_10,
            //     _ => 0,
            // };

            // FP damageAbility = targetHero.Hero.Level switch
            // {
            //     Hero.Level1 => 100,
            //     Hero.Level2 => 150,
            //     Hero.Level3 => 225,
            //     _ => 0,
            // };

            // HeroAttack.ApplyEffectToTarget(f, ref targetHero, board, ref targetHero, new HeroEffects.Effect()
            // {
            //     Type = HeroEffects.EffectType.FirebirdRebirth,
            //     MaxValue = attackSpeedIncrease,
            //     Value = damageAbility,
            //     Duration = duration,
            //     Size = 0,
            // });
        }

        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            // QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            // fightingHero = heroes[fightingHero.Index];
            // PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            // int heroLevel = fightingHero.Hero.Level;
            // SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            // FP damage = heroLevel switch
            // {
            //     Hero.Level1 => 350,
            //     Hero.Level2 => 525,
            //     Hero.Level3 => 800,
            //     _ => 0
            // };

            // FP reduceHealAndArmor = heroLevel switch
            // {
            //     Hero.Level1 => FP._0_20,
            //     Hero.Level2 => FP._0_20 + FP._0_10,
            //     Hero.Level3 => FP._0_20 * 2,
            //     _ => 0
            // };

            // if (fightingHero.ExtraLives > 0)
            // {
            //     return TryCastMain(f, fightingHero, board, damage, reduceHealAndArmor);
            // }
            // else
            // {
            //     return TryCastAfterRebirth(f, fightingHero, board, damage);
            // }

            return false;
        }

        private static bool TryCastMain(Frame f, FightingHero fightingHero, Board board, FP damage, FP reduceHealAndArmor)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.IncreaseHealAndArmor,
                    Value = 1 - reduceHealAndArmor,
                    Duration = 5,
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastAfterRebirth(Frame f, FightingHero fightingHero, Board board, FP damage)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.BlastStun,
                    Duration = 1,
                    Size = 2,
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}
