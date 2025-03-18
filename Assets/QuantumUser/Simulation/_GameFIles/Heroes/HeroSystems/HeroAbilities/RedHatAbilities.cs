using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class RedHatAbilities
    {
        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];

            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int secondHeroAbilityIndex = HeroAbility.GetSecondHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);
            int thirdHeroAbilityIndex = HeroAbility.GetThirdHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);

            if (secondHeroAbilityIndex < 0)
            {
                return TryCast_0_0(f, fightingHero, board);
                // return TryCast(f, fightingHero, board);
            }
            else if (thirdHeroAbilityIndex < 0)
            {
                if (secondHeroAbilityIndex == 0)
                {
                    return TryCast_0(f, fightingHero, board);
                }
                else if (secondHeroAbilityIndex == 1)
                {
                    return TryCast_1(f, fightingHero, board);
                }

                return false;
            }
            else
            {
                if (secondHeroAbilityIndex == 0)
                {
                    if (thirdHeroAbilityIndex == 0)
                    {
                        return TryCast_0_0(f, fightingHero, board);
                    }
                    else if (thirdHeroAbilityIndex == 1)
                    {
                    }
                    else if (thirdHeroAbilityIndex == 2)
                    {
                    }
                }
                else if (secondHeroAbilityIndex == 1)
                {
                    if (thirdHeroAbilityIndex == 0)
                    {
                    }
                    else if (thirdHeroAbilityIndex == 1)
                    {
                    }
                    else if (thirdHeroAbilityIndex == 2)
                    {
                    }
                }

                return false;
            }
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTarget(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 80;
                HeroAttack.ProjectileAttack(f, fightingHero, target, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCast_0(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTarget(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 120;
                HeroEffects.Effect effect = new()
                {
                    OwnerIndex = fightingHero.Index,
                    Type = HeroEffects.EffectType.Bleeding,
                    Value = (FP)50 / 3,
                    Duration = 3
                };

                HeroAbility.ProjectileAttack(f, fightingHero, target, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCast_1(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTarget(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 120;
                HeroEffects.Effect effect = new()
                {
                    OwnerIndex = fightingHero.Index,
                    Type = HeroEffects.EffectType.Curse,
                    Value = FP._0_75,
                    Duration = 3
                };

                HeroAbility.ProjectileAttack(f, fightingHero, target, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }


        private static bool TryCast_0_0(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAttack.TryFindClosestTarget(f, fightingHero, board, out FightingHero target))
            {
                FP damage = 160;
                HeroEffects.Effect effect1 = new()
                {
                    OwnerIndex = fightingHero.Index,
                    Type = HeroEffects.EffectType.Bleeding,
                    Value = (FP)75 / 3,
                    Duration = 3
                };

                HeroEffects.Effect effectw = new()
                {
                    OwnerIndex = fightingHero.Index,
                    Type = HeroEffects.EffectType.IncteaseTakingDamage,
                    Value = FP._1_20,
                    Duration = 3
                };

                HeroEffects.Effect[] effects = new HeroEffects.Effect[] { effect1, effectw };

                HeroAbility.ProjectileAttack(f, fightingHero, target, damage, effects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}