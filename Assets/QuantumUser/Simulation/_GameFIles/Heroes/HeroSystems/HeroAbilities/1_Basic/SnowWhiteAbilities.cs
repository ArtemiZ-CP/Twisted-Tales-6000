using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class SnowWhiteAbilities
    {
        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            // QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            // fightingHero = heroes[fightingHero.Index];
            // PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            // int heroLevel = fightingHero.Hero.Level;
            // SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            // if (heroLevel == Hero.Level1)
            // {
            //     return TryCast(f, fightingHero, board, 100);
            // }
            // else if (heroLevel == Hero.Level2)
            // {
            //     return TryCast(f, fightingHero, board, 150);
            // }
            // else if (heroLevel == Hero.Level3)
            // {
            //     return TryCast(f, fightingHero, board, 225);
            // }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP blastDamage)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP damage = blastDamage;
                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.Blast,
                    Value = blastDamage,
                    Size = 1,
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}