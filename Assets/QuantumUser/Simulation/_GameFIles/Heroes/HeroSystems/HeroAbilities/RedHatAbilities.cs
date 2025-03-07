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
                return TryCast_0(f, fightingHero, board);
            }
            else if (thirdHeroAbilityIndex < 0)
            {
                if (secondHeroAbilityIndex == 0)
                {
                    return TryCast_0(f, fightingHero, board);
                }
                else if (secondHeroAbilityIndex == 1)
                {

                }

                return false;
            }
            else
            {
                // Third level ability
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
    }
}