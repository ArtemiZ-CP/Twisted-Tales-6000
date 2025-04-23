using Photon.Deterministic;
using Quantum.Collections;

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
                return TryCast(f, fightingHero, board, 80, 20, 30);
            }
            else if (heroLevel == 1)
            {
                return TryCast(f, fightingHero, board, 120, 30, 45);
            }
            else if (heroLevel == 2)
            {
                return TryCast(f, fightingHero, board, 180, 45, 65);
            }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP damage, FP poisonDamage, FP healAmount)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.GlobalEffect poisonGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.PoisonArea,
                    Value = poisonDamage,
                    Duration = 3,
                    Size = 1
                };

                HeroEffects.GlobalEffect healGlobalEffect = new()
                {
                    Center = target.Index,
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.GlobalEffectType.HealArea,
                    Value = healAmount,
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