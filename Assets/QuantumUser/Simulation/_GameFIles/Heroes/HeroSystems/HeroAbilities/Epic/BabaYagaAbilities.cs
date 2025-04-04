using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class BabaYagaAbilities
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
                return TryCast(f, fightingHero, board, 140, 60);
            }
            else if (heroLevel == 1)
            {
                return TryCast(f, fightingHero, board, 210, 90);
            }
            else if (heroLevel == 2)
            {
                return TryCast(f, fightingHero, board, 315, 135);
            }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP damage, FP blastDamage)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.HorizontalBlast,
                    Value = blastDamage,
                    Size = 8,
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}