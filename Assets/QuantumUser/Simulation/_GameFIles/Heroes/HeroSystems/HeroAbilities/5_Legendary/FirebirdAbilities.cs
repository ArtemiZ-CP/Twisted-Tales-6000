using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class FirebirdAbilities
    {
        public const int Range = 2;

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
                0 => 350,
                1 => 525,
                2 => 800,
                _ => 0
            };

            FP reduceHealAndArmor = heroLevel switch
            {
                0 => FP._0_20,
                1 => FP._0_20 + FP._0_10,
                2 => FP._0_20 * 2,
                _ => 0
            };

            if (fightingHero.ExtraLives > 0)
            {
                return TryCastMain(f, fightingHero, board, damage, reduceHealAndArmor);
            }
            else
            {
                return TryCastAfterRebirth(f, fightingHero, board, damage);
            }
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
